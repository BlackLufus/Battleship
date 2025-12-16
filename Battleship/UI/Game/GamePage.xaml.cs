using Battleship.Lobby;
using Battleship.Logic.BattelStrategy;
using Battleship.Logic.Board;
using Battleship.Logic.Global;
using Battleship.Logic.Network;
using Battleship.Logic.Services;
using Battleship.Resources.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Battleship.UI.Game
{
    /// <summary>
    /// Interaktionslogik für GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        private readonly GameLogic gameLogic;

        private readonly GameSetting gameSetting;
        private readonly PlaygroundBoardLogic friendlyBoard;
        private readonly PlaygroundBoardLogic enemyBoard;

        private readonly StackPanel[,] friendlyCellPanel;
        private readonly CellState[,] friendlyCellState;
        private readonly StackPanel[,] enemyCellPanel;
        private readonly CellState[,] enemyCellState;

        private readonly ImageSource WaterImage =
            new BitmapImage(new Uri("pack://application:,,,/Resources/Images/water-field.png"));

        private readonly ImageSource FireImage =
            new BitmapImage(new Uri("pack://application:,,,/Resources/Images/fire.png"));

        private readonly ImageSource RedCrossImage =
            new BitmapImage(new Uri("pack://application:,,,/Resources/Images/red-cross.png"));

        private readonly ImageSource RestrictionImage =
            new BitmapImage(new Uri("pack://application:,,,/Resources/Images/restriction.png"));

        private readonly ImageSource TargetImage =
            new BitmapImage(new Uri("pack://application:,,,/Resources/Images/target.png"));

        public GamePage(GameSetting gameSetting, List<DragShip> friednlyDragShips, List<DragShip> enemyDragShips)
        {
            this.gameSetting = gameSetting;
            friendlyCellPanel = new StackPanel[gameSetting.BoardSize, gameSetting.BoardSize];
            friendlyCellState = new CellState[gameSetting.BoardSize, gameSetting.BoardSize];
            enemyCellPanel = new StackPanel[gameSetting.BoardSize, gameSetting.BoardSize];
            enemyCellState = new CellState[gameSetting.BoardSize, gameSetting.BoardSize];

            InitializeComponent();

            this.friendlyBoard = new PlaygroundBoardLogic(gameSetting.BoardSize, friednlyDragShips);
            this.friendlyBoard.Show(FriendlyShipCanvas, true);
            this.enemyBoard = new PlaygroundBoardLogic(gameSetting.BoardSize, enemyDragShips);
            this.enemyBoard.Show(EnemyShipCanvas, false);

            SetFriendlyUsername(Variables.Username);
            SetEnemyUsername($"Computer{new Random().Next(1000, 9999)}");

            InitPlaygroundBoard(EnemyWaterGrid, EnemyCellGrid, EnemyTargetGrid, enemyCellPanel, false);
            InitPlaygroundBoard(FriendlyWaterGrid, FriendlyCellGrid, FriendlyTargetGrid, friendlyCellPanel, true);

            gameLogic = new GameLogic(gameSetting, enemyBoard, friendlyBoard);
            gameLogic.OnEnemyShot += HandleEnemyShot;
            gameLogic.OnFriendlyShot += HandleFriendlyShot;
            gameLogic.OnGameEnded += HandleGameEnded;
            gameLogic.StartSinglePlayerMode();
        }

        public GamePage(ExchangeHandler exchangeHandler, GameSetting gameSetting, List<DragShip> friednlyDragShips, List<DragShip> enemyDragShips)
        {
            this.gameSetting = gameSetting;
            friendlyCellPanel = new StackPanel[gameSetting.BoardSize, gameSetting.BoardSize];
            friendlyCellState = new CellState[gameSetting.BoardSize, gameSetting.BoardSize];
            enemyCellPanel = new StackPanel[gameSetting.BoardSize, gameSetting.BoardSize];
            enemyCellState = new CellState[gameSetting.BoardSize, gameSetting.BoardSize];

            InitializeComponent();

            this.friendlyBoard = new PlaygroundBoardLogic(gameSetting.BoardSize, friednlyDragShips);
            this.friendlyBoard.Show(FriendlyShipCanvas, true);
            this.enemyBoard = new PlaygroundBoardLogic(gameSetting.BoardSize, enemyDragShips);
            this.enemyBoard.Show(EnemyShipCanvas, false);

            SetFriendlyUsername(exchangeHandler.Username);
            SetEnemyUsername(exchangeHandler.EnemyUsername!);

            InitPlaygroundBoard(EnemyWaterGrid, EnemyCellGrid, EnemyTargetGrid, enemyCellPanel, false);
            InitPlaygroundBoard(FriendlyWaterGrid, FriendlyCellGrid, FriendlyTargetGrid, friendlyCellPanel, true);

            gameLogic = new GameLogic(exchangeHandler, gameSetting, enemyBoard, friendlyBoard);
            gameLogic.OnEnemyShot += HandleEnemyShot;
            gameLogic.OnFriendlyShot += HandleFriendlyShot;
            gameLogic.OnGameEnded += HandleGameEnded;
            gameLogic.OnTimeout += HandleTimout;
            gameLogic.OnDisconnect += HandleDisconnet;
            gameLogic.StartMultiPlayerMode();
        }

        private void SetEnemyUsername(string username)
        {
            OpponentUsername.Content = username;
        }

        private void SetFriendlyUsername(string username)
        {
            MyUsername.Content = username;
        }

        private void InitPlaygroundBoard(Grid waterGrid, Grid CellState, Grid targetGrid, StackPanel[,] cellPanel, bool friendlyBoard)
        {
            // Erstelle 10 Zeilen und 10 Spalten
            for (int i = 0; i < gameSetting.BoardSize; i++)
            {
                waterGrid.RowDefinitions.Add(new RowDefinition());
                waterGrid.ColumnDefinitions.Add(new ColumnDefinition());
                CellState.RowDefinitions.Add(new RowDefinition());
                CellState.ColumnDefinitions.Add(new ColumnDefinition());
                targetGrid.RowDefinitions.Add(new RowDefinition());
                targetGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Füge in jede Zelle ein Bild hinzu
            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize; col++)
                {
                    // Water Grid
                    Image image = new()
                    {
                        Source = WaterImage
                    };

                    // Position in Grid
                    Grid.SetRow(image, row);
                    Grid.SetColumn(image, col);

                    // Add to Grid
                    waterGrid.Children.Add(image);


                    // Cell State Grid
                    StackPanel stackPanel = new StackPanel();
                    cellPanel[row, col] = stackPanel;

                    // Position in Grid
                    Grid.SetRow(stackPanel, row);
                    Grid.SetColumn(stackPanel, col);

                    // Add to Grid
                    CellState.Children.Add(stackPanel);


                    // Target Grid
                    Border cell = new Border()
                    {
                        Tag = new Point(row, col),
                        Background = Brushes.Transparent
                    };

                    // Add Event listeners
                    cell.MouseEnter += OnMouseEnter;
                    cell.MouseLeave += OnMouseLeave;
                    if (!friendlyBoard)
                    {
                        cell.PreviewMouseDown += OnPreviewMouseDown;
                        cell.PreviewMouseUp += OnPreviewMouseUp;
                    }

                    // Position in Grid
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);

                    // Add to Grid
                    targetGrid.Children.Add(cell);
                }
            }
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            Border cell = (Border)sender;
            var image = new Image
            {
                Source = TargetImage,
                Stretch = Stretch.Uniform
            };
            cell.Child = image;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            Border cell = (Border)sender;
            cell.Background = Brushes.Transparent;
            cell.Child = null;
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Border cell = (Border)sender;
            Point p = (Point)cell.Tag;
            gameLogic.Shoot((int)p.X, (int)p.Y);
            cell.Background = new BrushConverter().ConvertFrom("#44FFFFFF") as Brush;
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Border cell = (Border)sender;
            cell.Background = Brushes.Transparent;
        }

        private void UpdateCell(StackPanel element, CellState state)
        {
            if (state == CellState.HIT || state == CellState.SUNK)
            {
                Image image = new()
                {
                    Source = FireImage,
                };
                element.Children.Add(image);
            }
            else if (state == CellState.MISS)
            {
                Image image = new()
                {
                    Source = RedCrossImage,
                };
                element.Children.Add(image);
            }
            else if (state == CellState.BLOCKED && gameSetting.RestrictedArea)
            {
                Image image = new()
                {
                    Source = RestrictionImage,
                };
                element.Children.Add(image);
            }
        }

        private void UpdateUI(CellState[,] newCellState, CellState[,] currentCellState, StackPanel[,] cellPanels)
        {
            for (int x = 0; x < gameSetting.BoardSize; x++)
            {
                for (int y = 0; y < gameSetting.BoardSize; y++)
                {
                    // Update content of a specific Cell
                    if (currentCellState[x, y] != newCellState[x, y])
                    {
                        currentCellState[x, y] = newCellState[x, y];
                        UpdateCell(cellPanels[x, y], newCellState[x, y]);
                    }
                }
            }
        }

        private void HandleEnemyShot()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CellState[,] newCellState = enemyBoard.GetBoardState();
                UpdateUI(newCellState, enemyCellState, enemyCellPanel);
            });
        }

        private void HandleFriendlyShot()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CellState[,] newCellState = friendlyBoard.GetBoardState();
                UpdateUI(newCellState, friendlyCellState, friendlyCellPanel);
            });
        }

        private void HandleGameEnded(bool hasWon)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string title = "Das Spiel ist zu Ende";
                string content = hasWon
                    ? $"Herzlichen Glückwunsch {MyUsername.Content}!\nDu hast das Spiel gewonnen."
                    : $"Leider Verloren!\n{OpponentUsername.Content} hat das Spiel gewonnen.";
                Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, title, content, (Dialog.Result result) =>
                {
                    Debug.WriteLine($"Result clicked: {result}");

                    // Navigate Menu page
                    Navigation.RegisterPage(new LobbyMainPage(false));
                });
            });
        }

        private void HandleTimout()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Show dialog to inform user
                Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Timeout", "Dein Gegner antwortet nicht mehr.", (Dialog.Result result) =>
                {
                    // Navigate Menu page
                    Navigation.RegisterPage(new LobbyMainPage(false));
                });
            });
        }

        private void HandleDisconnet()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Show dialog to inform user
                Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Spiel zu Ende", "Dein Gegner hat das Spiel verlassen", (Dialog.Result result) =>
                {
                    // Navigate Menu page
                    Navigation.RegisterPage(new LobbyMainPage(false));
                });
            });
        }
    }
}
