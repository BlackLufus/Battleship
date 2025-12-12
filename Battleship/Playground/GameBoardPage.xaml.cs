using Battelship;
using Battleship.Global;
using Battleship.Lobby;
using Battleship.Network;
using Battleship.Resources.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using System.Xml.Linq;
using static Battleship.Playground.GameLogic;

namespace Battleship.Playground
{
    /// <summary>
    /// Interaktionslogik für PlaygroundPage.xaml
    /// </summary>
    public partial class GameBoardPage : Page
    {
        private readonly MQTTService? mqttService;
        private readonly GameSetting gameSetting;
        private readonly Playground myPlayground;
        private readonly Playground enemyPlayground;

        private readonly GameLogic gameLogic;

        public GameBoardPage(MQTTService mqttService, GameSetting gameSetting, Playground myPlayground, bool myTurn = false)
        {
            this.gameSetting = gameSetting;
            this.myPlayground = myPlayground;
            this.enemyPlayground = new Playground(gameSetting.BoardSize, []);
            this.mqttService = mqttService;

            InitializeComponent();

            SetOpponentUsername(mqttService.OpponentUsername);
            SetMyUsername(Variables.Username);

            PrepareGame();

            PlaceShips(myPlayground.Ships, true);

            gameLogic = new GameLogic(gameSetting, MyFieldForeground, myPlayground, EnemyFieldForeground, enemyPlayground, mqttService);
            gameLogic.ShotEvent += PlaygroundUI.UpdatePlayground;
            gameLogic.GameEndedEvent += GameEnded;
        }

        public GameBoardPage(GameSetting gameSetting, Playground myPlayground, Playground enemyPlayground)
        {
            this.gameSetting = gameSetting;
            this.myPlayground = myPlayground;
            this.enemyPlayground = enemyPlayground;

            gameLogic = new GameLogic(gameSetting, enemyPlayground);
            gameLogic.ShotEvent += PlaygroundUI.UpdatePlayground;
            gameLogic.GameEndedEvent += GameEnded;

            InitializeComponent();

            PrepareGame();

            PlaceShips(myPlayground.Ships, true);

            PlaceShips(enemyPlayground.Ships, false);
            gameLogic.StartGame(MyFieldForeground, myPlayground, GameLogic.TurnType.EnemyTurn);
            if (gameSetting.GameMode == GameSetting.Mode.ComputerVsComputer) gameLogic.StartGame(EnemyFieldForeground, enemyPlayground, GameLogic.TurnType.MyTurn);
        }

        private void SetOpponentUsername(string username)
        {
            OpponentUsername.Content = username;
        }

        private void SetMyUsername(string username)
        {
            MyUsername.Content = username;
        }

        private void PrepareGame()
        {
            CleanUp();
            SetupPlaygroundDefinitions();
            InitializeEnemyPlayground();
            InitializePlayerPlayground();
        }

        private void CleanUp()
        {
            EnemyFieldBackground.Children.Clear();
            EnemyFieldBackground.RowDefinitions.Clear();
            EnemyFieldBackground.ColumnDefinitions.Clear();
            EnemyFieldForeground.Children.Clear();
            EnemyFieldForeground.RowDefinitions.Clear();
            EnemyFieldForeground.ColumnDefinitions.Clear();
            EnemyFieldTarget.Children.Clear();
            EnemyFieldTarget.RowDefinitions.Clear();
            EnemyFieldTarget.ColumnDefinitions.Clear();
            MyFieldBackground.Children.Clear();
            MyFieldBackground.RowDefinitions.Clear();
            MyFieldBackground.ColumnDefinitions.Clear();
            MyFieldForeground.Children.Clear();
            MyFieldForeground.RowDefinitions.Clear();
            MyFieldForeground.ColumnDefinitions.Clear();
            MyFieldTarget.Children.Clear();
            MyFieldTarget.RowDefinitions.Clear();
            MyFieldTarget.ColumnDefinitions.Clear();
        }

        private void SetupPlaygroundDefinitions()
        {
            for (int i = 0; i < gameSetting.BoardSize; i++)
            {
                EnemyFieldBackground.RowDefinitions.Add(new RowDefinition());
                EnemyFieldBackground.ColumnDefinitions.Add(new ColumnDefinition());
                EnemyFieldForeground.RowDefinitions.Add(new RowDefinition());
                EnemyFieldForeground.ColumnDefinitions.Add(new ColumnDefinition());
                EnemyFieldTarget.RowDefinitions.Add(new RowDefinition());
                EnemyFieldTarget.ColumnDefinitions.Add(new ColumnDefinition());
                MyFieldBackground.RowDefinitions.Add(new RowDefinition());
                MyFieldBackground.ColumnDefinitions.Add(new ColumnDefinition());
                MyFieldForeground.RowDefinitions.Add(new RowDefinition());
                MyFieldForeground.ColumnDefinitions.Add(new ColumnDefinition());
                MyFieldTarget.RowDefinitions.Add(new RowDefinition());
                MyFieldTarget.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        private void InitializeEnemyPlayground()
        {
            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize; col++)
                {

                    // Erstelle ein Image
                    Image enemieImage = new();
                    BitmapImage enemieImageBitmap = new(new Uri("pack://application:,,,/Resources/Images/water-field.png"));
                    enemieImage.Source = enemieImageBitmap;
                    Grid.SetRow(enemieImage, row);
                    Grid.SetColumn(enemieImage, col);
                    EnemyFieldBackground.Children.Add(enemieImage);

                    // Enemy Field Foreground
                    Button enemyField = new()
                    {
                        Height = gameSetting.CellSize,
                        Width = gameSetting.CellSize,
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(0)
                    };
                    Grid.SetRow(enemyField, row);
                    Grid.SetColumn(enemyField, col);
                    EnemyFieldForeground.Children.Add(enemyField);


                    // Enemy Field Target
                    Button enemyFieldTarget = new()
                    {
                        Height = gameSetting.CellSize,
                        Width = gameSetting.CellSize,
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(0)
                    };

                    enemyFieldTarget.MouseEnter += (sender, e) =>
                    {
                        int row = Grid.GetRow(enemyFieldTarget);
                        int col = Grid.GetColumn(enemyFieldTarget);
                        Grid grid = new();
                        grid.Children.Add(new Image
                        {
                            Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/target.png"))
                        });
                        enemyFieldTarget.Content = grid;
                    };
                    enemyFieldTarget.MouseLeave += (sender, e) =>
                    {
                        int row = Grid.GetRow(enemyFieldTarget);
                        int col = Grid.GetColumn(enemyFieldTarget);
                        if (enemyPlayground.Field[row, col] != 0 && enemyPlayground.Field[row, col] != 1)
                        {
                            Grid grid = new()
                            {
                                Background = Brushes.Transparent,
                            };
                            enemyFieldTarget.Content = grid;
                        }
                        else
                        {
                            enemyFieldTarget.Content = null;
                        }
                    };
                    enemyFieldTarget.PreviewMouseDown += (sender, e) =>
                    {
                        enemyFieldTarget.Background = new BrushConverter().ConvertFrom("#44FFFFFF") as Brush;
                    };
                    enemyFieldTarget.PreviewMouseUp += (sender, e) =>
                    {
                        enemyFieldTarget.Background = Brushes.Transparent;
                    };
                    enemyFieldTarget.Click += (sender, e) =>
                    {
                        if (gameSetting.GameMode == GameSetting.Mode.PlayerVsPlayer)
                        {
                            gameLogic!.Shot(EnemyFieldForeground, enemyField);
                        }
                        else
                        {
                            gameLogic!.Shot(EnemyFieldForeground, enemyField);
                        }
                    };
                    Grid.SetRow(enemyFieldTarget, row);
                    Grid.SetColumn(enemyFieldTarget, col);
                    EnemyFieldTarget.Children.Add(enemyFieldTarget);
                }
            }
        }

        private void InitializePlayerPlayground()
        {
            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize; col++)
                {
                    // My Field Background
                    Image myImage = new();
                    BitmapImage myBitmap = new(new Uri("pack://application:,,,/Resources/Images/water-field.png"));
                    myImage.Source = myBitmap;
                    Grid.SetRow(myImage, row);
                    Grid.SetColumn(myImage, col);
                    MyFieldBackground.Children.Add(myImage);


                    // My Field Foreground
                    Button myField = new()
                    {
                        Height = gameSetting.CellSize,
                        Width = gameSetting.CellSize,
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(0)
                    };
                    Grid.SetRow(myField, row);
                    Grid.SetColumn(myField, col);
                    MyFieldForeground.Children.Add(myField);


                    // My Field Target
                    Button myFieldTarget = new()
                    {
                        Height = gameSetting.CellSize,
                        Width = gameSetting.CellSize,
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(0)
                    };
                    myFieldTarget.MouseEnter += (sender, e) =>
                    {
                        int row = Grid.GetRow(myFieldTarget);
                        int col = Grid.GetColumn(myFieldTarget);
                        Grid grid = new();
                        grid.Children.Add(new Image
                        {
                            Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/target.png"))
                        });
                        myFieldTarget.Content = grid;
                    };
                    myFieldTarget.MouseLeave += (sender, e) =>
                    {
                        int row = Grid.GetRow(myFieldTarget);
                        int col = Grid.GetColumn(myFieldTarget);
                        if (myPlayground.Field[row, col] != 0 && myPlayground.Field[row, col] != 1)
                        {
                            Grid grid = new()
                            {
                                Background = Brushes.Transparent,
                            };
                            myFieldTarget.Content = grid;
                        }
                        else
                        {
                            myFieldTarget.Content = null;
                        }
                    };
                    myFieldTarget.PreviewMouseDown += (sender, e) =>
                    {
                        myFieldTarget.Background = new BrushConverter().ConvertFrom("#44FFFFFF") as Brush;
                    };
                    myFieldTarget.PreviewMouseUp += (sender, e) =>
                    {
                        myFieldTarget.Background = Brushes.Transparent;
                    };
                    Grid.SetRow(myFieldTarget, row);
                    Grid.SetColumn(myFieldTarget, col);
                    MyFieldTarget.Children.Add(myFieldTarget);
                }
            }
        }

        private static TransformedBitmap RotateImage(BitmapSource source, double angle)
        {
            TransformedBitmap transform = new();
            transform.BeginInit();
            transform.Source = source;
            transform.Transform = new RotateTransform(angle);
            transform.EndInit();
            return transform;
        }

        private void PlaceShips(List<Ship> ships, bool myField)
        {
            // Set the ships on the playground
            foreach (Ship ship in ships!)
            {
                Image element = new()
                {
                    Width = gameSetting.CellSize * (ship.shipOrientation == Ship.ShipOrientation.Horizontal ? (int)ship.shipType : 1),
                    Height = gameSetting.CellSize * (ship.shipOrientation == Ship.ShipOrientation.Vertical ? (int)ship.shipType : 1),
                    Source = RotateImage(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + ship.shipType.ToString().ToLower() + ".png")), (int)ship.shipOrientation)
                };

                if (myField)
                {
                    MyField.Children.Add(element);
                }
                else
                {
                    EnemyField.Children.Add(element);
                }

                Canvas.SetLeft(element, 5 + ship.column * gameSetting.CellSize);
                Canvas.SetTop(element, ship.row * gameSetting.CellSize);
            }
        }

        private void GameEnded(TurnType turn)
        {
            Debug.WriteLine("Game ended");
            Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Game ended", "The game has ended\n" + (turn == TurnType.MyTurn ? "You won" : "You lose"), (result) =>
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Navigation.RegisterPage(new LobbyMainPage(false));
                    });
                }
                else
                {
                    Navigation.RegisterPage(new LobbyMainPage(false));
                }
            });
        }
    }
}
