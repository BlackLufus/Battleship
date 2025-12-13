using Battleship.Logic;
using Battleship.Logic.BattelStrategy;
using Battleship.Logic.Global;
using Battleship.Logic.Network;
using System;
using System.Collections.Generic;
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

namespace Battleship.UI.Game
{
    /// <summary>
    /// Interaktionslogik für GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        private readonly MQTTService? mqttService;
        private readonly GameSetting gameSetting;
        private readonly BoardLogic friendlyBoard;
        private readonly BoardLogic enemyBoard;

        private readonly GameLogic gameLogic;

        public GamePage(MQTTService mqttService, GameSetting gameSetting, BoardLogic friendlyBoard, bool myTurn = false)
        {
            this.gameSetting = gameSetting;
            this.friendlyBoard = friendlyBoard;
            this.enemyBoard = new BoardLogic(gameSetting.BoardSize, []);
            this.mqttService = mqttService;

            InitializeComponent();

            SetOpponentUsername(mqttService.OpponentUsername);
            SetMyUsername(Variables.Username);

            PrepareGame();
        }

        public GamePage(GameSetting gameSetting, BoardLogic friendlyBoard, BoardLogic enemyBoard)
        {
            this.gameSetting = gameSetting;
            this.friendlyBoard = friendlyBoard;
            this.enemyBoard = enemyBoard;

            InitializeComponent();

            PrepareGame();
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
                        //if (enemyPlayground.Field[row, col] != 0 && enemyPlayground.Field[row, col] != 1)
                        //{
                        //    Grid grid = new()
                        //    {
                        //        Background = Brushes.Transparent,
                        //    };
                        //    enemyFieldTarget.Content = grid;
                        //}
                        //else
                        //{
                        //    enemyFieldTarget.Content = null;
                        //}
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
                        //if (friendlyBoard.Field[row, col] != 0 && friendlyBoard.Field[row, col] != 1)
                        //{
                        //    Grid grid = new()
                        //    {
                        //        Background = Brushes.Transparent,
                        //    };
                        //    myFieldTarget.Content = grid;
                        //}
                        //else
                        //{
                        //    myFieldTarget.Content = null;
                        //}
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
    }
}
