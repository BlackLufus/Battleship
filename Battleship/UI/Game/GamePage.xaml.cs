using Battleship.Logic;
using Battleship.Logic.BattelStrategy;
using Battleship.Logic.Global;
using Battleship.Logic.Network;
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
        private readonly MQTTService? mqttService;
        private readonly GameSetting gameSetting;
        private readonly PlaygroundBoardLogic friendlyBoard;
        private readonly PlaygroundBoardLogic enemyBoard;

        private readonly GameLogic gameLogic;

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
            this.friendlyBoard.Show(FriendlyShipCanvas);
            this.enemyBoard = new PlaygroundBoardLogic(gameSetting.BoardSize, enemyDragShips);
            this.enemyBoard.Show(EnemyShipCanvas);

            InitPlaygroundBoard(EnemyWaterGrid, EnemyCellGrid, EnemyTargetGrid, enemyCellPanel);
            InitPlaygroundBoard(FriendlyWaterGrid, FriendlyCellGrid, FriendlyTargetGrid, friendlyCellPanel);

            gameLogic = new GameLogic(gameSetting, enemyBoard, friendlyBoard);
            gameLogic.OnEnemyShotEvent += OnEnemyShotEvent;
            gameLogic.OnFriendlyShotEvent += OnFriendlyShotEvent;
            gameLogic.OnGameEndedEvent += OnGameEndedEvent;
            gameLogic.Start();
        }

        private void SetOpponentUsername(string username)
        {
            OpponentUsername.Content = username;
        }

        private void SetMyUsername(string username)
        {
            MyUsername.Content = username;
        }

        private void InitPlaygroundBoard(Grid waterGrid, Grid CellState, Grid targetGrid, StackPanel[,] cellPanel)
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
                    cell.PreviewMouseDown += OnPreviewMouseDown;
                    cell.PreviewMouseUp += OnPreviewMouseUp;

                    // Position in Grid
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);

                    // Add to Grid
                    targetGrid.Children.Add(cell);
                }
            }
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
            Border cell = (Border) sender;
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

        private void OnEnemyShotEvent()
        {
            CellState[,] newCellState = enemyBoard.GetBoardState();
            update(newCellState, enemyCellState, enemyCellPanel);
        }

        private void OnFriendlyShotEvent()
        {
            CellState[,] newCellState = friendlyBoard.GetBoardState();
            update(newCellState, friendlyCellState, friendlyCellPanel);
        }

        private void update(CellState[,] newCellState, CellState[,] currentCellState, StackPanel[,] cellPanels)
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

        private void OnGameEndedEvent()
        {
            Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Game Over", "Das spiel ist zu ende, danke fürs Spielen");
        }
    }
}
