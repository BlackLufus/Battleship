using Battleship.Global;
using Battleship.Lobby;
using Battleship.Playground;
using Battleship.services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
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

namespace Battelship.Lobby
{

    /// <summary>
    /// Interaktionslogik für FleetManager.xaml
    /// </summary>
    public partial class FleetManagerPage : Page
    {

        private GameSetting gameSetting;
        DragAndDropManager dragAndDropManager;

        public FleetManagerPage(GameSetting gameSetting)
        {
            this.gameSetting = gameSetting;
            InitializeComponent();
            this.dragAndDropManager = new DragAndDropManager(ShipsCanvas, DragAndDropGrid, gameSetting);
            CreateImageGrid();
        }

        private void CreateImageGrid()
        {
            // Erstelle 10 Zeilen und 10 Spalten
            for (int i = 0; i < gameSetting.FieldSize; i++)
            {
                ImageGrid.RowDefinitions.Add(new RowDefinition());
                ImageGrid.ColumnDefinitions.Add(new ColumnDefinition());
                DragAndDropGrid.RowDefinitions.Add(new RowDefinition());
                DragAndDropGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Füge in jede Zelle ein Bild hinzu
            for (int row = 0; row < gameSetting.FieldSize; row++)
            {
                for (int col = 0; col < gameSetting.FieldSize; col++)
                {
                    // Erstelle ein Image
                    Image image = new Image();

                    // Lade das Bild (hier ein Beispielbild aus dem Projektverzeichnis)
                    BitmapImage bitmap = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/water-field.png"));
                    image.Source = bitmap;

                    // Setze das Bild in die entsprechende Zelle
                    Grid.SetRow(image, row);
                    Grid.SetColumn(image, col);

                    // Füge das Bild zum Grid hinzu
                    ImageGrid.Children.Add(image);

                    StackPanel stackPanel = new StackPanel();
                    var bc = new BrushConverter();
                    stackPanel.Background = bc.ConvertFrom("#22000000") as Brush;
                    stackPanel.AllowDrop = true;
                    stackPanel.AddHandler(DragOverEvent, new DragEventHandler(dragAndDropManager.HandleDragOverEvent));
                    stackPanel.AddHandler(DragLeaveEvent, new DragEventHandler(dragAndDropManager.HandleDragLeaveEvent));
                    stackPanel.AddHandler(DropEvent, new DragEventHandler(dragAndDropManager.HandleDropEvent));
                    stackPanel.MouseRightButtonDown += dragAndDropManager.RemoveShip;
                    stackPanel.MouseLeftButtonDown += dragAndDropManager.RedragShip;
                    Grid.SetRow(stackPanel, row);
                    Grid.SetColumn(stackPanel, col);
                    DragAndDropGrid.Children.Add(stackPanel);
                }
            }
        }

        private void finishButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.RegisterPage(new PlaygroundPage(gameSetting, dragAndDropManager.DragShips));
        }

        private void randomButton_Click(object sender, RoutedEventArgs e)
        {
            dragAndDropManager.Randomize();
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            dragAndDropManager.Reset();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateBack();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateTo(new MenuPage());
        }

        private void DragShip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch(((Image)sender).Name.ToLower())
            {
                case "carrier":
                    if (dragAndDropManager.CarrierPlaced != gameSetting.CarrierAmount)
                        dragAndDropManager.StartDrag(Ship.ShipType.Carrier, e.GetPosition((Image)sender));
                    break;
                case "battleship":
                    if (dragAndDropManager.BattleshipPlaced != gameSetting.BattleshipAmount)
                        dragAndDropManager.StartDrag(Ship.ShipType.Battleship, e.GetPosition((Image)sender));
                    break;
                case "cruiser":
                    if (dragAndDropManager.CruiserPlaced != gameSetting.CruiserAmount)
                        dragAndDropManager.StartDrag(Ship.ShipType.Cruiser, e.GetPosition((Image)sender));
                    break;
                case "submarine":
                    if (dragAndDropManager.SubmarinePlaced != gameSetting.SubmarineAmount)
                        dragAndDropManager.StartDrag(Ship.ShipType.Submarine, e.GetPosition((Image)sender));
                    break;
                case "destroyer":
                    if (dragAndDropManager.DestroyerPlaced != gameSetting.DestroyerAmount)
                        dragAndDropManager.StartDrag(Ship.ShipType.Destroyer, e.GetPosition((Image)sender));
                    break;
                default:
                    Debug.WriteLine("Unknown ship");
                    break;
            }
        }

        private void Page_DragOver(object sender, DragEventArgs e)
        {
            Point mousePosition = e.GetPosition(ShipsCanvas);
            dragAndDropManager.MoveDragShip(mousePosition);
        }
    }
}
