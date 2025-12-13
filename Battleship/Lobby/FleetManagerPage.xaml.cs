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

        private FieldState[,] cellState;
        private StackPanel[,] cellPanels;

        private readonly DragShipManager dragShipManager;
        private readonly List<DragShip> dragShips = [];

        private readonly Brush WaterBrush = (Brush)new BrushConverter().ConvertFrom("#22000000");
        private readonly Brush MarkedBrush = (Brush)new BrushConverter().ConvertFrom("#44ff0000");

        private readonly ImageSource RestrictionImage =
    new BitmapImage(new Uri("pack://application:,,,/Resources/Images/restriction.png"));

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

            // Initialize component
            InitializeComponent();

            // Adjust buttons for PvP mode
            if (gameSetting.GameMode == GameSetting.Mode.PlayerVsPlayer)
            {
                CancelButton.Visibility = Visibility.Visible;
                BackButton.Visibility = Visibility.Collapsed;
            }

            // Initialize Battlefield
            this.battleField = new BattleField(gameSetting.BoardSize);
            battleField.OnStateChangeEvent += Update;

            // Initialize DragShipManager
            this.dragShipManager = new DragShipManager(gameSetting.CellSize, battleField);

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
            for (int i = 0; i < gameSetting.BoardSize; i++)
            {
                ImageGrid.RowDefinitions.Add(new RowDefinition());
                ImageGrid.ColumnDefinitions.Add(new ColumnDefinition());
                DragAndDropGrid.RowDefinitions.Add(new RowDefinition());
                DragAndDropGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            cellState = new FieldState[gameSetting.BoardSize, gameSetting.BoardSize];
            cellPanels = new StackPanel[gameSetting.BoardSize, gameSetting.BoardSize];

            // Füge in jede Zelle ein Bild hinzu
            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize; col++)
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
                    cellPanels[row, col] = stackPanel;
                    var bc = new BrushConverter();
                    stackPanel.Background = WaterBrush;
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
            var basicDragShips = GetBasicShipList();
            foreach (DragShip dragShip in basicDragShips)
            {
                dragShips.Add(dragShip);
                dragShip.Show(dragShipManager);
            }
        }

        private List<DragShip> GetBasicShipList()
        {
            List<DragShip> tempDragShips = [];
            for (int i = 0; i < gameSetting.BattleshipAmount; i++)
                tempDragShips.Add(new DragShip(GameCanvas, gameSetting.CellSize, Ship.ShipType.Battleship));
            for (int i = 0; i < gameSetting.CruiserAmount; i++)
                tempDragShips.Add(new DragShip(GameCanvas, gameSetting.CellSize, Ship.ShipType.Cruiser));
            for (int i = 0; i < gameSetting.SubmarineAmount; i++)
                tempDragShips.Add(new DragShip(GameCanvas, gameSetting.CellSize, Ship.ShipType.Submarine));
            for (int i = 0; i < gameSetting.DestroyerAmount; i++)
                tempDragShips.Add(new DragShip(GameCanvas, gameSetting.CellSize, Ship.ShipType.Destroyer));

            return tempDragShips;
        }

        /// <summary>
        /// Reset the battlefield and draggers
        /// </summary>
        private void Reset()
        {
            battleField.Reset();
            Debug.WriteLine($"Delete Ships: {dragShips.Count}");
            foreach (var dragShip in dragShips)
            {
                Debug.WriteLine("Delete");
                GameCanvas.Children.Remove(dragShip.img);
            }
            dragShips.Clear();
            Update();
        }

        /// <summary>
        /// Aktualisiert das Spielfeld basierend auf dem aktuellen Zustand des BattleFields
        /// </summary>
        private void Update()
        {
            var boardState = battleField.GetBoardState();
            for (int x = 0; x < gameSetting.BoardSize; x++)
            {
                for (int y = 0; y < gameSetting.BoardSize; y++)
                {
                    // Update content of a specific Cell
                    if (cellState[x, y] != boardState[x, y])
                    {
                        cellState[x, y] = boardState[x, y];
                        UpdateCell(cellPanels[x, y], boardState[x, y]);
                    }
                }
            }
        }

        /// <summary>
        /// Updates a single board cell based on its field state
        /// </summary>
        /// <param name="element">The StackPanel element representing the cell</param>
        /// <param name="fieldState">The state of the field</param>
        private void UpdateCell(StackPanel element, BattleField.FieldState fieldState)
        {
            if (fieldState == BattleField.FieldState.Marked)
            {
                element.Background = MarkedBrush;
                element.Children.Clear();
            }
            else if (fieldState == BattleField.FieldState.Water)
            {
                element.Background = WaterBrush;
                element.Children.Clear();
            }
            else if (fieldState == BattleField.FieldState.Ship)
            {
                element.Background = WaterBrush;
                element.Children.Clear();
            }
            else
            {
                element.Background = WaterBrush;
                Image image = new()
                {
                    Source = RestrictionImage,
                    Margin = new Thickness(4, 4, 4, 4),
                };
                element.Children.Add(image);
            }
        }

        /// <summary>
        /// Event handler for the Finish button click
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            List<Ship> friendlyShips = dragShips.Cast<Ship>().ToList();
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
                        Navigation.RegisterPage(new GameBoardPage(mqttService, gameSetting, new Playground(gameSetting.BoardSize, friendlyShips)));
                    });
                };
                mqttService.SendReady();
            }
            else
            {
                BattleField enemyBattleField = new BattleField(gameSetting.BoardSize);
                var basicShipList = GetBasicShipList();
                
                bool isRandomizedSucceed = false;

                do
                    isRandomizedSucceed = enemyBattleField.Randomize(basicShipList.Cast<Ship>().ToList());
                while (!isRandomizedSucceed);

                Navigation.RegisterPage(new GameBoardPage(gameSetting, new Playground(gameSetting.BoardSize, friendlyShips), new Playground(gameSetting.BoardSize, basicShipList.Cast<Ship>().ToList())));
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
            List<DragShip> basicDragShips = GetBasicShipList();
            bool isRandomizedSucceed = battleField.Randomize(basicDragShips.Cast<Ship>().ToList());
            if (!isRandomizedSucceed)
            {
                Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Ups!", "Das hat leider nicht geklappt, versuch es noch mal oder ändere die Anzahl an Verfügbaren Schiffen!");
                return;
            }
            foreach (DragShip dragShip in basicDragShips)
            {
                // Orientierung setzen
                dragShips.Add(dragShip);
                dragShip.Show(dragShipManager);
                dragShip.Rotate(dragShip.orientation);
                dragShip.Place();
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
    }
}
