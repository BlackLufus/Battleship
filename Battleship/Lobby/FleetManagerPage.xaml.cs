using Battleship.Global;
using Battleship.Lobby;
using Battleship.Network;
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
using static Battleship.Global.Ship;
using static Battleship.Lobby.BattleField;

namespace Battelship.Lobby
{

    /// <summary>
    /// Interaktionslogik für FleetManager.xaml
    /// </summary>
    public partial class FleetManagerPage : Page
    {
        private readonly MQTTService? mqttService;

        private readonly GameSetting gameSetting;
        private readonly BattleField battleField;

        private readonly DragShipManager simpleDragDrop;
        private readonly List<Dragger> draggers = [];

        public FleetManagerPage(GameSetting gameSetting, MQTTService? mqttService = null)
        {
            this.gameSetting = gameSetting;
            this.mqttService = mqttService;

            // Handle disconnection event
            if (mqttService != null)
            {
                mqttService.OnDisconnected += () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Disconnected", "The other player has disconnected");
                        Navigation.NavigateAndClear(new MenuPage());
                    });
                };
            }

            InitializeComponent();

            // Adjust buttons for PvP mode
            if (gameSetting.GameMode == GameSetting.Mode.PlayerVsPlayer)
            {
                CancelButton.Visibility = Visibility.Visible;
                BackButton.Visibility = Visibility.Collapsed;
            }

            // Initialize Battlefield
            this.battleField = new BattleField(gameSetting.FieldSize);
            battleField.OnChangedEvent += HandleOnChange;

            // Simple Drag and Drop Manager
            this.simpleDragDrop = new DragShipManager(gameSetting.SingleFieldSize, battleField, GameCanvas);
            this.simpleDragDrop.OnShipRemovedEvent += HangleOnShipRemoved;

            // Initialize Field and Ships
            InitField();
            InitShips();
        }

        /// <summary>
        /// Initialisiert das Spielfeld mit Bildern
        /// </summary>
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
                    Grid.SetRow(stackPanel, row);
                    Grid.SetColumn(stackPanel, col);
                    DragAndDropGrid.Children.Add(stackPanel);
                }
            }
        }

        /// <summary>
        /// Initialisiert die Schiffe basierend auf den Spieleinstellungen
        /// </summary>
        private void InitShips()
        {
            for (int i = 0; i < gameSetting.BattleshipAmount; i++)
            {
                AddDragShip(Ship.ShipType.Battleship);
            }
            for (int i = 0; i < gameSetting.CruiserAmount; i++)
            {
                AddDragShip(Ship.ShipType.Cruiser);
            }
            for (int i = 0; i < gameSetting.SubmarineAmount; i++)
            {
                AddDragShip(Ship.ShipType.Submarine);
            }
            for (int i = 0; i < gameSetting.DestroyerAmount; i++)
            {
                AddDragShip(Ship.ShipType.Destroyer);
            }
        }

        /// <summary>
        /// Reset the battlefield and draggers
        /// </summary>
        private void Reset()
        {
            battleField.Reset();
            foreach (var dragger in draggers)
            {
                GameCanvas.Children.Remove(dragger.element);
            }
            Update();
        }

        /// <summary>
        /// Aktualisiert das Spielfeld basierend auf dem aktuellen Zustand des BattleFields
        /// </summary>
        private void Update()
        {
            var boardState = battleField.GetBoardState();
            for (int x = 0; x < gameSetting.FieldSize; x++)
            {
                for (int y = 0; y < gameSetting.FieldSize; y++)
                {
                    // Find the StackPanel at the specific row and column
                    StackPanel stackPanel = (StackPanel)DragAndDropGrid.Children.Cast<UIElement>().First(e => Grid.GetRow(e) == x && Grid.GetColumn(e) == y);
                    UpdateBoard(stackPanel, boardState[x, y]);
                }
            }
        }

        /// <summary>
        /// Updates a single board cell based on its field state
        /// </summary>
        /// <param name="element">The StackPanel element representing the cell</param>
        /// <param name="fieldState">The state of the field</param>
        private static void UpdateBoard(StackPanel element, BattleField.FieldState fieldState)
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

        /// <summary>
        /// Returns a list of ships based on the game settings
        /// </summary>
        /// <returns>The list of ships</returns>
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

        /// <summary>
        /// Event handler for the Finish button click
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            BattleField enemyBattleField = new BattleField(gameSetting.FieldSize);
            List<Ship> ships = GetShipList();
            // enemyBattleField.Randomize(ships);
            Debug.WriteLine("FinishButton clicked");
            Debug.WriteLine("mqttService: " + mqttService);
            if (mqttService != null)
            {
                if (!mqttService.IsOpponentReady)
                {
                    Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Warte auf Gegner", "Warte bis der Gegner fertig ist");
                }
                mqttService.OnStartGame += () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Navigation.RegisterPage(new GameBoardPage(mqttService, gameSetting, new Playground(gameSetting.FieldSize, battleField.ships)));
                    });
                };
                mqttService.SendReady();
            }
            else
            {
                enemyBattleField.Randomize(ships);
                Navigation.RegisterPage(new GameBoardPage(gameSetting, new Playground(gameSetting.FieldSize, battleField.ships), new Playground(gameSetting.FieldSize, enemyBattleField.ships)));
            }
        }

        /// <summary>
        /// Event handler for the Random button click
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            List<Ship>? ships = battleField.Randomize(GetShipList());
            draggers.Clear();
            if (ships == null)
            {
                Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Ups!", "Das hat leier nicht geklappt, versuch es noch mal oder setzt die Anzahl an Schiffen runter!");
                return;
            }
            foreach (Ship ship in ships)
            {
                Dragger dragger = AddDragShip(ship.shipType);
                draggers.Add(dragger);

                // Orientierung setzen
                dragger.orientation = ship.shipOrientation;

                // Größe anpassen nach Orientation
                if (ship.shipOrientation == Ship.ShipOrientation.Horizontal)
                {
                    dragger.element.Width = gameSetting.SingleFieldSize * (int)ship.shipType;
                    dragger.element.Height = gameSetting.SingleFieldSize;
                }
                else
                {
                    dragger.element.Width = gameSetting.SingleFieldSize;
                    dragger.element.Height = gameSetting.SingleFieldSize * (int)ship.shipType;
                }

                // Bild richtig drehen
                dragger.element.Source = DragShipManager.RotateImage(
                    new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + ship.shipType.ToString().ToLower() + ".png")),
                    (int)ship.shipOrientation
                );

                // Position im Canvas korrekt setzen (Column = X, Row = Y)
                double x = ship.column * gameSetting.SingleFieldSize;
                double y = ship.row * gameSetting.SingleFieldSize;

                Canvas.SetLeft(dragger.element, x);
                Canvas.SetTop(dragger.element, y);

                // Mausposition auf die neue Position setzen (NICHT row/column vertauschen!)
                dragger.mousePos = new Point(x, y);
            }
            Update();
        }

        /// <summary>
        /// Event handler for the Reset button click
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            InitShips();
        }

        /// <summary>
        /// Event handler for the Back button click
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateBack();
        }

        /// <summary>
        /// Event handler for the Cancel button click
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateAndClear(new MenuPage());
            mqttService?.Disconnected();
        }

        /// <summary>
        /// Event handler for when the battlefield changes
        /// </summary>
        private void HandleOnChange()
        {
            Update();
        }

        /// <summary>
        /// Event handler for when a ship is removed from the battlefield
        /// </summary>
        /// <param name="dragger"></param>
        private void HangleOnShipRemoved(Dragger dragger)
        {
            if (dragger != null)
            {
                GameCanvas.Children.Remove(dragger.element);
                draggers.Remove(dragger);
            }
            Update();
        }

        /// <summary>
        /// Adds a draggable ship to the canvas
        /// </summary>
        /// <param name="type">The type of the ship</param>
        /// <returns>The created Dragger object</returns>
        private Dragger AddDragShip(Ship.ShipType type)
        {
            var img = new Image()
            {
                Width = gameSetting.SingleFieldSize * (int)type,
                Height = gameSetting.SingleFieldSize,
                Source = DragShipManager.RotateImage(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + type.ToString().ToLower() + ".png")), 270)
            };

            var dragShip = new Dragger(img, type);
            draggers.Add(dragShip);

            GameCanvas.Children.Add(img);

            Canvas.SetTop(img, dragShip.startTopOffset);
            Canvas.SetLeft(img, dragShip.startLeftOffset);

            img.MouseLeftButtonDown += (s, e) =>
            {
                Debug.WriteLine("Start Dragging");
                simpleDragDrop.StartDrag(dragShip, e.GetPosition(DragAndDropGrid));
            };

            img.MouseMove += (s, e) =>
            {
                if (dragShip == simpleDragDrop.currentDrag)
                    simpleDragDrop.Move(e.GetPosition(DragAndDropGrid));
            };

            img.MouseLeftButtonUp += (s, e) =>
            {
                simpleDragDrop.EndDrag(e.GetPosition(DragAndDropGrid));
            };

            // rotate with right click
            img.MouseRightButtonDown += (s, e) =>
            {
                simpleDragDrop.RotateCurrent();
            };

            return dragShip;
        }
    }
}
