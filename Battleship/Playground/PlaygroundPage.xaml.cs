using Battelship;
using Battleship.Global;
using Battleship.Lobby;
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

namespace Battleship.Playground
{
    /// <summary>
    /// Interaktionslogik für PlaygroundPage.xaml
    /// </summary>
    public partial class PlaygroundPage : Page
    {
        private readonly GameSetting gameSetting;
        private readonly BattleshipPlayground myPlayground;
        private readonly BattleshipPlayground enemyPlayground;

        private enum Turn
        {
            MyTurn = 0,
            EnemyTurn = 1
        }
        private Turn turn = new Random().Next(0, 2) == 0 ? Turn.MyTurn : Turn.EnemyTurn;

        public PlaygroundPage(GameSetting gameSetting, BattleshipPlayground myPlayground, BattleshipPlayground enemyPlayground)
        {
            this.gameSetting = gameSetting;
            this.myPlayground = myPlayground;
            this.enemyPlayground = enemyPlayground;

            InitializeComponent();

            SetBackground();
            SetShips(myPlayground.Ships, true);
            if (gameSetting.GameMode != GameSetting.Mode.PlayerVsPlayer) SetShips(enemyPlayground.Ships, false);
            if (gameSetting.GameMode != GameSetting.Mode.PlayerVsPlayer) StartGame(MyFieldForeground, myPlayground, Turn.EnemyTurn);
            Debug.WriteLine("GameMode: " + gameSetting.GameMode);
            if (gameSetting.GameMode == GameSetting.Mode.ComputerVsComputer) StartGame(EnemyFieldForeground, enemyPlayground, Turn.MyTurn);
        }

        private void SetBackground()
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

            Debug.WriteLine(gameSetting.FieldSize);

            for (int i = 0; i < gameSetting.FieldSize; i++)
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

            for (int row = 0; row < gameSetting.FieldSize; row++)
            {
                for (int col = 0; col < gameSetting.FieldSize; col++)
                {

                    // Erstelle ein Image
                    Image enemieImage = new Image();
                    BitmapImage enemieImageBitmap = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/water-field.png"));
                    enemieImage.Source = enemieImageBitmap;
                    Grid.SetRow(enemieImage, row);
                    Grid.SetColumn(enemieImage, col);
                    EnemyFieldBackground.Children.Add(enemieImage);

                    // Enemy Field Foreground
                    Button enemyField = new Button
                    {
                        Height = gameSetting.SingleFieldSize,
                        Width = gameSetting.SingleFieldSize,
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(0)
                    };
                    Grid.SetRow(enemyField, row);
                    Grid.SetColumn(enemyField, col);
                    EnemyFieldForeground.Children.Add(enemyField);


                    // Enemy Field Target
                    Button enemyFieldTarget = new Button
                    {
                        Height = gameSetting.SingleFieldSize,
                        Width = gameSetting.SingleFieldSize,
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
                        Shot(enemyField);
                    };
                    Grid.SetRow(enemyFieldTarget, row);
                    Grid.SetColumn(enemyFieldTarget, col);
                    EnemyFieldTarget.Children.Add(enemyFieldTarget);



                    // My Field Background
                    Image myImage = new Image();
                    BitmapImage myBitmap = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/water-field.png"));
                    myImage.Source = myBitmap;
                    Grid.SetRow(myImage, row);
                    Grid.SetColumn(myImage, col);
                    MyFieldBackground.Children.Add(myImage);


                    // My Field Foreground
                    Button myField = new Button
                    {
                        Height = gameSetting.SingleFieldSize,
                        Width = gameSetting.SingleFieldSize,
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(0)
                    };
                    Grid.SetRow(myField, row);
                    Grid.SetColumn(myField, col);
                    MyFieldForeground.Children.Add(myField);


                    // My Field Target
                    Button myFieldTarget = new Button
                    {
                        Height = gameSetting.SingleFieldSize,
                        Width = gameSetting.SingleFieldSize,
                        Background = Brushes.Transparent,
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(0)
                    };
                    myFieldTarget.MouseEnter += (sender, e) =>
                    {
                        int row = Grid.GetRow(myFieldTarget);
                        int col = Grid.GetColumn(myFieldTarget);
                        Grid grid = new()
                        {
                            Background = new BrushConverter().ConvertFrom("#44FFFFFF") as Brush
                        };
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

        private void SetShips(List<Ship> ships, bool myField)
        {
            // Set the ships on the playground
            foreach (Ship ship in ships!)
            {
                Image element = new()
                {
                    Width = gameSetting.SingleFieldSize * (ship.shipOrientation == Ship.ShipOrientation.Horizontal ? (int)ship.shipType : 1),
                    Height = gameSetting.SingleFieldSize * (ship.shipOrientation == Ship.ShipOrientation.Vertical ? (int)ship.shipType : 1),
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

                Canvas.SetLeft(element, 5 + ship.column * gameSetting.SingleFieldSize);
                Canvas.SetTop(element, ship.row * gameSetting.SingleFieldSize);
            }
        }

        private void MarkField(Button element, BattleshipPlayground.FieldState shotResult)
        {
            if (shotResult == BattleshipPlayground.FieldState.Hit || shotResult == BattleshipPlayground.FieldState.Sunk)
            {
                Image image = new()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/fire.png")),
                    Width = gameSetting.SingleFieldSize - 2,
                    Height = gameSetting.SingleFieldSize - 2,
                };
                element.Content = image;
            }
            else if (shotResult == BattleshipPlayground.FieldState.Miss)
            {
                Image image = new()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/red-cross.png")),
                    Width = gameSetting.SingleFieldSize - 2,
                    Height = gameSetting.SingleFieldSize - 2,
                };
                element.Content = image;
            }
            else if (shotResult == BattleshipPlayground.FieldState.Restrict)
            {
                Image image = new()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/restriction.png")),
                    Width = gameSetting.SingleFieldSize - 2,
                    Height = gameSetting.SingleFieldSize - 2,
                };
                element.Content = image;
            }
        }
        private void UpdateField(Grid playgroundGrid, BattleshipPlayground playground)
        {
            for (int row = 0; row < gameSetting.FieldSize; row++)
            {
                for (int col = 0; col < gameSetting.FieldSize; col++)
                {
                    Button button = (Button)playgroundGrid.Children.Cast<UIElement>().First(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);
                    MarkField(button, (BattleshipPlayground.FieldState)playground.Field[row, col]);
                }
            }
        }

        private void StartGame(Grid playgroundGrid, BattleshipPlayground playground, Turn turn)
        {
            AI ai = new AI(gameSetting, playground);
            bool gameEnded = false;
            Thread thread = new Thread(() =>
            {
                while (!gameEnded)
                {
                    if (this.turn == turn)
                    {
                        try
                        {
                            Thread.Sleep(new Random().Next(75, 100));
                            bool hit = ai.NextShot();
                            Debug.WriteLine("Hit: " + hit);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                UpdateField(playgroundGrid, playground);
                            });
                            if (!gameSetting.HitBonus || !hit)
                            {
                                this.turn = this.turn == Turn.MyTurn ? Turn.EnemyTurn : Turn.MyTurn;
                            }
                            if (playground.IsAllSunk())
                            {
                                gameEnded = true;
                            }
                        }
                        catch (ThreadInterruptedException _) { }
                    }
                    else
                    {
                        try
                        {
                            Thread.Sleep(75);
                        }
                        catch (ThreadInterruptedException _) { }
                    }
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GameEnded();
                });
            });
            thread.Start();
        }

        private void Shot(Button enemyField)
        {
            if (gameSetting.GameMode != GameSetting.Mode.ComputerVsComputer && this.turn == Turn.MyTurn)
            {
                int row = Grid.GetRow(enemyField);
                int col = Grid.GetColumn(enemyField);
                
                BattleshipPlayground.ShotResult shotResult = enemyPlayground.Shot(row, col);
                
                Debug.WriteLine("Shot at " + row + " " + col + " with result " + shotResult);

                UpdateField(EnemyFieldForeground, enemyPlayground);

                if (enemyPlayground.IsAllSunk())
                {
                    GameEnded();
                }
                else if (!gameSetting.HitBonus || shotResult == BattleshipPlayground.ShotResult.Miss)
                {
                    this.turn = Turn.EnemyTurn;
                }
            }
        }

        private void GameEnded()
        {
            Debug.WriteLine("Game ended");
            Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Game ended", "The game has ended\n" + (this.turn == Turn.MyTurn ? "You won" : "You lose"), (result) =>
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
