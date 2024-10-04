using Battleship.Lobby;
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
    public partial class FleetManager : Page
    {

        private static FleetManager? instance;

        DragAndDropManager dragAndDropManager;

        private FleetManager()
        {
            InitializeComponent();
            this.dragAndDropManager = new DragAndDropManager(ShipsCanvas, DragAndDropGrid, 10);
            CreateImageGrid();
        }

        public static FleetManager get()
        {
            if (instance == null)
            {
                instance = new FleetManager();
            }
            return instance;
        }

        private void CreateImageGrid()
        {
            // Erstelle 10 Zeilen und 10 Spalten
            for (int i = 0; i < 10; i++)
            {
                ImageGrid.RowDefinitions.Add(new RowDefinition());
                ImageGrid.ColumnDefinitions.Add(new ColumnDefinition());
                DragAndDropGrid.RowDefinitions.Add(new RowDefinition());
                DragAndDropGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Füge in jede Zelle ein Bild hinzu
            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
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
                    stackPanel.Background = (Brush)bc.ConvertFrom("#22000000");
                    stackPanel.AllowDrop = true;
                    stackPanel.AddHandler(DragOverEvent, new DragEventHandler(dragAndDropManager.HandleDragOverEvent));
                    stackPanel.AddHandler(DragLeaveEvent, new DragEventHandler(dragAndDropManager.HandleDragLeaveEvent));
                    stackPanel.AddHandler(DropEvent, new DragEventHandler(dragAndDropManager.HandleDropEvent));
                    Grid.SetRow(stackPanel, row);
                    Grid.SetColumn(stackPanel, col);
                    DragAndDropGrid.Children.Add(stackPanel);
                }
            }
        }

        private void finishButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void randomButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.navigateBack();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.navigateTo(StartMenu.get());
        }

        private void DragShip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Mouse down" + ((Image)sender).Name);
            switch(((Image)sender).Name.ToLower())
            {
                case "carrier":
                    dragAndDropManager.StartDrag(Ship.ShipType.Carrier, e.GetPosition((Image)sender));
                    break;
                case "battleship":
                    dragAndDropManager.StartDrag(Ship.ShipType.Battleship, e.GetPosition((Image)sender));
                    break;
                case "cruiser":
                    dragAndDropManager.StartDrag(Ship.ShipType.Cruiser, e.GetPosition((Image)sender));
                    break;
                case "submarine":
                    dragAndDropManager.StartDrag(Ship.ShipType.Submarine, e.GetPosition((Image)sender));
                    break;
                case "destroyer":
                    dragAndDropManager.StartDrag(Ship.ShipType.Destroyer, e.GetPosition((Image)sender));
                    break;
                default:
                    Debug.WriteLine("Unknown ship");
                    break;
            }
            /*if (sender is UIElement element) 
            {
                Debug.WriteLine("Mouse down");
                dragAndDropManager.StartDrag(element, e.GetPosition(element));
            }*/
        }



        private void Page_DragOver(object sender, DragEventArgs e)
        {
            Point mousePosition = e.GetPosition(ShipsCanvas);
            dragAndDropManager.MoveDragShip(mousePosition);
        }

        private void DragShip_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Mouse up");
            dragAndDropManager.EndDrag();
        }

        private void MainWindow_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // Wenn die linke Maustaste losgelassen wird und das Ziel nicht erreicht wird
            if (e.Action == DragAction.Cancel || (e.KeyStates & DragDropKeyStates.LeftMouseButton) == 0)
            {
                // Der Drag-Vorgang wurde außerhalb eines Ziehziels abgebrochen
                Debug.WriteLine("Drag and Drop canceled!");
                e.Action = DragAction.Cancel;
            }
        }
    }
}
