using Battleship.Lobby;
using Battleship.Playground;
using Battleship.Resources.Components;
using Battleship.services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
using static Battleship.Lobby.BattleField;

namespace Battelship.Lobby
{

    /// <summary>
    /// Interaktionslogik für FleetManager.xaml
    /// </summary>
    public partial class FleetManagerPage : Page
    {

        private readonly GameSetting gameSetting;
        private readonly BattleField battleField;
        private readonly DragAndDropManager dragAndDropManager;

        private int carrierPlaced = 0;
        private int battleshipPlaced = 0;
        private int cruiserPlaced = 0;
        private int submarinePlaced = 0;
        private int destroyerPlaced = 0;
        public int CarrierPlaced => carrierPlaced;
        public int BattleshipPlaced => battleshipPlaced;
        public int CruiserPlaced => cruiserPlaced;
        public int SubmarinePlaced => submarinePlaced;
        public int DestroyerPlaced => destroyerPlaced;

        public FleetManagerPage(GameSetting gameSetting)
        {
            this.gameSetting = gameSetting;

            InitializeComponent();

            if (gameSetting.GameMode == GameSetting.Mode.PlayerVsPlayer)
            {
                CancelButton.Visibility = Visibility.Visible;
                BackButton.Visibility = Visibility.Collapsed;
            }

            this.battleField = new BattleField(gameSetting);

            this.dragAndDropManager = new DragAndDropManager(gameSetting, battleField);
            this.dragAndDropManager.DragEnterEvent += TriggerDragEnterEvent;
            this.dragAndDropManager.DragLeaveEvent += TriggerDragLeaveEvent;
            this.dragAndDropManager.DragDropEvent += TriggerDragDropEvent;
            this.dragAndDropManager.UpdateEvent += Update;
            this.dragAndDropManager.AddShipEvent += AddShip;
            this.dragAndDropManager.RemoveShipEvent += RemoveShip;

            InitField();
        }

        private void InitField()
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
                    Image image = new();

                    // Lade das Bild (hier ein Beispielbild aus dem Projektverzeichnis)
                    BitmapImage bitmap = new(new Uri("pack://application:,,,/Resources/Images/water-field.png"));
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

        private void TriggerDragEnterEvent(DragShip dragShip, int row, int column)
        {
            if (battleField.SetShip(
                dragShip.shipOrientation == DragShip.ShipOrientation.Vertical ? row -= (int)(dragShip.offset.Y / gameSetting.SingleFieldSize) : row,
                dragShip.shipOrientation == DragShip.ShipOrientation.Horizontal ? column -= (int)(dragShip.offset.X / gameSetting.SingleFieldSize) : column,
                dragShip.shipType,
                dragShip.shipOrientation,
                FieldState.Marked))
            {
                return;
            }
            else
            {
                Canvas.SetLeft(dragShip.element, 190 + column * gameSetting.SingleFieldSize);
                Canvas.SetTop(dragShip.element, 0 + row * gameSetting.SingleFieldSize);
            }
            Update();
        }

        private void TriggerDragLeaveEvent(DragShip dragShip, int row, int column)
        {
            if (!battleField.RemoveShip(
                    dragShip.shipOrientation == DragShip.ShipOrientation.Vertical ? row -= (int)(dragShip.offset.Y / gameSetting.SingleFieldSize) : row,
                    dragShip.shipOrientation == DragShip.ShipOrientation.Horizontal ? column -= (int)(dragShip.offset.X / gameSetting.SingleFieldSize) : column,
                    dragShip.shipType,
                    dragShip.shipOrientation,
                    FieldState.Marked))
            {
                return;
            }
            Update();
        }

        private void TriggerDragDropEvent(DragShip dragShip, int row, int column, Action<int, int> callback)
        {
            if (!battleField.SetShip(
                dragShip.shipOrientation == DragShip.ShipOrientation.Vertical ? row -= (int)(dragShip.offset.Y / gameSetting.SingleFieldSize) : row,
                dragShip.shipOrientation == DragShip.ShipOrientation.Horizontal ? column -= (int)(dragShip.offset.X / gameSetting.SingleFieldSize) : column,
                dragShip.shipType,
                dragShip.shipOrientation,
                FieldState.Ship))
            {
                return;
            }
            else
            {
                Canvas.SetTop(dragShip.element, 0 + row * gameSetting.SingleFieldSize);
                Canvas.SetLeft(dragShip.element, 190 + column * gameSetting.SingleFieldSize);
                AddShip(dragShip.element);
                callback(row, column);
            }
            Update();
        }

        private void Update()
        {
            for (int x = 0; x < gameSetting.FieldSize; x++)
            {
                for (int y = 0; y < gameSetting.FieldSize; y++)
                {
                    StackPanel stackPanel = (StackPanel)DragAndDropGrid.Children.Cast<UIElement>().First(e => Grid.GetRow(e) == x && Grid.GetColumn(e) == y);
                    MarkField(stackPanel, (BattleField.FieldState)battleField.Field[x, y]);
                }
            }
            UpdateShipsInfo();
        }

        private void AddShip(UIElement element)
        {
            ShipsCanvas.Children.Add(element);
        }

        private void RemoveShip(UIElement element)
        {
            ShipsCanvas.Children.Remove(element);
        }

        private static void MarkField(StackPanel element, BattleField.FieldState fieldState)
        {
            if (fieldState == BattleField.FieldState.Marked)
            {
                //element.Background = Brushes.Green;
                element.Background = new BrushConverter().ConvertFrom("#44ff0000") as Brush;
                element.Children.Clear();
            }
            else if (fieldState == BattleField.FieldState.Water)
            {
                element.Background = new BrushConverter().ConvertFrom("#22000000") as Brush;
                element.Children.Clear();
            }
            else if (fieldState == BattleField.FieldState.Ship)
            {
                //element.Background = Brushes.Red;
                element.Background = new BrushConverter().ConvertFrom("#22000000") as Brush;
                element.Children.Clear();
            }
            else
            {
                element.Background = new BrushConverter().ConvertFrom("#22000000") as Brush;
                Image image = new()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/restriction.png")),
                    Margin = new Thickness(4, 4, 4, 4),
                };
                element.Children.Add(image);
            }
        }

        private void UpdateShipsInfo()
        {
            carrierPlaced = dragAndDropManager.DragShips.Count(dragShip => dragShip.shipType == Ship.ShipType.Carrier);
            battleshipPlaced = dragAndDropManager.DragShips.Count(dragShip => dragShip.shipType == Ship.ShipType.Battleship);
            cruiserPlaced = dragAndDropManager.DragShips.Count(dragShip => dragShip.shipType == Ship.ShipType.Cruiser);
            submarinePlaced = dragAndDropManager.DragShips.Count(dragShip => dragShip.shipType == Ship.ShipType.Submarine);
            destroyerPlaced = dragAndDropManager.DragShips.Count(dragShip => dragShip.shipType == Ship.ShipType.Destroyer);
        }

        public List<Ship> GetShipList()
        {
            List<Ship> shipList = [];
            for (int i = 0; i < gameSetting.BattleshipAmount; i++)
            {
                Ship.ShipType shipType = Ship.ShipType.Battleship;
                Ship ship = new(shipType, Ship.ShipOrientation.Horizontal);
                shipList.Add(ship);
            }
            for (int i = 0; i < gameSetting.CruiserAmount; i++)
            {
                Ship.ShipType shipType = Ship.ShipType.Cruiser;
                Ship ship = new(shipType, Ship.ShipOrientation.Horizontal);
                shipList.Add(ship);
            }
            for (int i = 0; i < gameSetting.SubmarineAmount; i++)
            {
                Ship.ShipType shipType = Ship.ShipType.Submarine;
                Ship ship = new(shipType, Ship.ShipOrientation.Horizontal);
                shipList.Add(ship);
            }
            for (int i = 0; i < gameSetting.DestroyerAmount; i++)
            {
                Ship.ShipType shipType = Ship.ShipType.Destroyer;
                Ship ship = new(shipType, Ship.ShipOrientation.Horizontal);
                shipList.Add(ship);
            }

            return shipList;
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            BattleField enemyBattleField = new BattleField(gameSetting);
            List<Ship> ships = GetShipList();
            enemyBattleField.Randomize(ships);
            Navigation.RegisterPage(new GameBoardPage(gameSetting, new Playground(battleField.FieldNoRestiction, dragAndDropManager.DragShips.Select(ship => ship as Ship).ToList()), gameSetting.GameMode == GameSetting.Mode.PlayerVsPlayer ? null : new Playground(enemyBattleField.FieldNoRestiction, ships)));
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            dragAndDropManager.Randomize(GetShipList());
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            dragAndDropManager.Reset();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateAndClear(new MenuPage());
        }

        private void DragShip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch(((Image)sender).Name.ToLower())
            {
                case "carrier":
                    if (CarrierPlaced != gameSetting.CarrierAmount)
                        dragAndDropManager.StartDrag(Ship.ShipType.Carrier, e.GetPosition((Image)sender));
                    break;
                case "battleship":
                    if (BattleshipPlaced != gameSetting.BattleshipAmount)
                        dragAndDropManager.StartDrag(Ship.ShipType.Battleship, e.GetPosition((Image)sender));
                    break;
                case "cruiser":
                    if (CruiserPlaced != gameSetting.CruiserAmount)
                        dragAndDropManager.StartDrag(Ship.ShipType.Cruiser, e.GetPosition((Image)sender));
                    break;
                case "submarine":
                    if (SubmarinePlaced != gameSetting.SubmarineAmount)
                        dragAndDropManager.StartDrag(Ship.ShipType.Submarine, e.GetPosition((Image)sender));
                    break;
                case "destroyer":
                    if (DestroyerPlaced != gameSetting.DestroyerAmount)
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
