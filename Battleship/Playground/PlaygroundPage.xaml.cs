using Battleship.Global;
using Battleship.Lobby;
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

namespace Battleship.Playground
{
    /// <summary>
    /// Interaktionslogik für PlaygroundPage.xaml
    /// </summary>
    public partial class PlaygroundPage : Page
    {
        private readonly GameSetting gameSetting;
        private readonly List<DragShip> ships;

        public PlaygroundPage(GameSetting gameSetting, List<DragShip> ships)
        {
            this.gameSetting = gameSetting;
            this.ships = ships;

            InitializeComponent();

            SetBackground();
            SetShips();
        }

        public List<Ship> Ships { get; }

        private void SetBackground()
        {
            EnemyFieldBackground.Children.Clear();
            EnemyFieldBackground.RowDefinitions.Clear();
            EnemyFieldBackground.ColumnDefinitions.Clear();
            EnemyFieldForeground.Children.Clear();
            EnemyFieldForeground.RowDefinitions.Clear();
            EnemyFieldForeground.ColumnDefinitions.Clear();
            MyFieldBackground.Children.Clear();
            MyFieldBackground.RowDefinitions.Clear();
            MyFieldBackground.ColumnDefinitions.Clear();
            MyFieldForeground.Children.Clear();
            MyFieldForeground.RowDefinitions.Clear();
            MyFieldForeground.ColumnDefinitions.Clear();

            Debug.WriteLine(gameSetting.FieldSize);

            for (int i = 0; i < gameSetting.FieldSize; i++)
            {
                EnemyFieldBackground.RowDefinitions.Add(new RowDefinition());
                EnemyFieldBackground.ColumnDefinitions.Add(new ColumnDefinition());
                EnemyFieldForeground.RowDefinitions.Add(new RowDefinition());
                EnemyFieldForeground.ColumnDefinitions.Add(new ColumnDefinition());
                MyFieldBackground.RowDefinitions.Add(new RowDefinition());
                MyFieldBackground.ColumnDefinitions.Add(new ColumnDefinition());
                MyFieldForeground.RowDefinitions.Add(new RowDefinition());
                MyFieldForeground.ColumnDefinitions.Add(new ColumnDefinition());
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

                    // Create a separate Image for EnemyFieldForeground
                    StackPanel enemyFieldGrid = new StackPanel();
                    enemyFieldGrid.Background = Brushes.Transparent;
                    enemyFieldGrid.MouseEnter += (sender, e) =>
                    { 
                        Image enemyImage = new Image();
                        BitmapImage enemyImageBitmap = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/target.png"));
                        enemyImage.Source = enemyImageBitmap;
                        enemyFieldGrid.Children.Add(enemyImage);
                    };
                    enemyFieldGrid.MouseLeave += (sender, e) =>
                    {
                        enemyFieldGrid.Children.Clear();
                        enemyFieldGrid.Background = Brushes.Transparent;
                    };
                    enemyFieldGrid.MouseDown += (sender, e) =>
                    {
                        enemyFieldGrid.Background = new BrushConverter().ConvertFrom("#22FFFFFF") as Brush;
                    };
                    enemyFieldGrid.MouseUp += (sender, e) =>
                    {
                        enemyFieldGrid.Background = Brushes.Transparent;
                    };
                    Grid.SetRow(enemyFieldGrid, row);
                    Grid.SetColumn(enemyFieldGrid, col);
                    EnemyFieldForeground.Children.Add(enemyFieldGrid);

                    // Create a separate Image for MyFieldBackground
                    Image myImage = new Image();
                    BitmapImage myBitmap = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/water-field.png"));
                    myImage.Source = myBitmap;
                    Grid.SetRow(myImage, row);
                    Grid.SetColumn(myImage, col);
                    MyFieldBackground.Children.Add(myImage);

                    // Create a separate Image for MyFieldForeground
                    StackPanel myFieldGrid = new StackPanel();
                    myFieldGrid.Background = Brushes.Transparent;
                    myFieldGrid.MouseEnter += (sender, e) =>
                    {
                        Image myImage = new Image();
                        BitmapImage myImageBitmap = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/target.png"));
                        myImage.Source = myImageBitmap;
                        myFieldGrid.Children.Add(myImage);
                    };
                    myFieldGrid.MouseLeave += (sender, e) =>
                    {
                        myFieldGrid.Children.Clear();
                        myFieldGrid.Background = Brushes.Transparent;
                    };
                    myFieldGrid.MouseDown += (sender, e) =>
                    {
                        myFieldGrid.Background = new BrushConverter().ConvertFrom("#22FFFFFF") as Brush;
                    };
                    myFieldGrid.MouseUp += (sender, e) =>
                    {
                        myFieldGrid.Background = Brushes.Transparent;
                    };
                    Grid.SetRow(myFieldGrid, row);
                    Grid.SetColumn(myFieldGrid, col);
                    MyFieldForeground.Children.Add(myFieldGrid);
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

        private void SetShips()
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

                MyField.Children.Add(element);

                Canvas.SetLeft(element, 5 + ship.column * gameSetting.SingleFieldSize);
                Canvas.SetTop(element, ship.row * gameSetting.SingleFieldSize);
            }
        }
    }
}
