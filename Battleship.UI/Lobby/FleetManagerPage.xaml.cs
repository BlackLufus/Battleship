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
using Battleship.Core.Game;
using Battleship.Core.Global;
using Battleship.Core.Network;
using Battleship.UI.Game;
using Battleship.UI.Lobby;
using Battleship.UI.Navi;
using Battleship.UI.Resources.Components;

namespace Battelship.UI.Lobby
{
    /// <summary>
    /// Interaktionslogik für FleetManager.xaml
    /// </summary>
    public partial class FleetManagerPage : Page
    {
        private readonly ExchangeHandler? exchangeHandler;

        private readonly GameSetting gameSetting;
        private readonly PlacementGrid battleField;

        private readonly FieldState[,] cellState;
        private readonly StackPanel[,] cellPanels;

        private readonly DragShipManager dragShipManager;
        private readonly List<DragShip> dragShips = [];

        private readonly Brush WaterBrush = (Brush)new BrushConverter().ConvertFrom("#22000000");
        private readonly Brush MarkedBrush = (Brush)new BrushConverter().ConvertFrom("#44ff0000");

        private readonly ImageSource RestrictionImage = new BitmapImage(
            new Uri("pack://application:,,,/Resources/Images/restriction.png")
        );

        private readonly ImageSource WaterImage = new BitmapImage(
            new Uri("pack://application:,,,/Resources/Images/water-field.png")
        );

        public FleetManagerPage(GameSetting gameSetting, ExchangeHandler? exchangeHandler = null)
        {
            this.gameSetting = gameSetting;

            // Handle disconnection event
            if (exchangeHandler != null)
            {
                this.exchangeHandler = exchangeHandler;
                this.exchangeHandler.OnTimeout += HandleTimeout;
                this.exchangeHandler.OnDisconnect += HandleDisconnect;
                this.exchangeHandler.OnReady += HandleReadyState;
                this.exchangeHandler.OnReadyAck += HandleReadyState;
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
            this.battleField = new PlacementGrid(gameSetting.BoardSize);
            battleField.OnStateChangeEvent += Update;

            // Initialize DragShipManager
            this.dragShipManager = new DragShipManager(gameSetting.CellSize, battleField);

            cellState = new FieldState[gameSetting.BoardSize, gameSetting.BoardSize];
            cellPanels = new StackPanel[gameSetting.BoardSize, gameSetting.BoardSize];

            // Initialize Field and Ships
            InitField();
            InitShips();
        }

        /// <summary>
        /// Initialisiert das Spielfeld mit Bildern
        /// </summary>
        private void InitField()
        {
            // Add Row and Column Definitions
            for (int i = 0; i < gameSetting.BoardSize; i++)
            {
                ImageGrid.RowDefinitions.Add(new RowDefinition());
                ImageGrid.ColumnDefinitions.Add(new ColumnDefinition());
                DragAndDropGrid.RowDefinitions.Add(new RowDefinition());
                DragAndDropGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Füge in jede Zelle ein Bild hinzu
            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize; col++)
                {
                    // Water Grid
                    Image image = new Image { Source = WaterImage };

                    // Position in Grid
                    Grid.SetRow(image, row);
                    Grid.SetColumn(image, col);

                    // Add to Grid
                    ImageGrid.Children.Add(image);

                    // Cell State Grid
                    StackPanel stackPanel = new StackPanel() { Background = WaterBrush };
                    cellPanels[row, col] = stackPanel;

                    // Position in Grid
                    Grid.SetRow(stackPanel, row);
                    Grid.SetColumn(stackPanel, col);

                    // Add to Grid
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

        /// <summary>
        /// Get basic drag ships, depending on settings in gameSettings object
        /// </summary>
        /// <returns>A list of DragShip objects</returns>
        private List<DragShip> GetBasicShipList()
        {
            List<DragShip> tempDragShips = [];
            for (int i = 0; i < gameSetting.BattleshipAmount; i++)
                tempDragShips.Add(
                    new DragShip(GameCanvas, gameSetting.CellSize, ShipType.Battleship)
                );
            for (int i = 0; i < gameSetting.CruiserAmount; i++)
                tempDragShips.Add(new DragShip(GameCanvas, gameSetting.CellSize, ShipType.Cruiser));
            for (int i = 0; i < gameSetting.SubmarineAmount; i++)
                tempDragShips.Add(
                    new DragShip(GameCanvas, gameSetting.CellSize, ShipType.Submarine)
                );
            for (int i = 0; i < gameSetting.DestroyerAmount; i++)
                tempDragShips.Add(
                    new DragShip(GameCanvas, gameSetting.CellSize, ShipType.Destroyer)
                );

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
                dragShip.Dispose();
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
        private void UpdateCell(StackPanel element, FieldState fieldState)
        {
            if (fieldState == FieldState.Marked)
            {
                element.Background = MarkedBrush;
                element.Children.Clear();
            }
            else if (fieldState == FieldState.Water)
            {
                element.Background = WaterBrush;
                element.Children.Clear();
            }
            else if (fieldState == FieldState.Ship)
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

        private void HandleTimeout()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Dispose exchange handler
                exchangeHandler?.Close();

                // Show dialog to inform user
                Dialog.Show(
                    Dialog.DialogType.Info,
                    Dialog.ButtonType.Ok,
                    "Timeout",
                    "Dein Gegner antwortet nicht mehr.",
                    (Dialog.Result result) =>
                    {
                        // Navigate back to previous page
                        Navigation.NavigateAndClear(new MenuPage());
                    }
                );
            });
        }

        private void HandleDisconnect()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Dispose exchange handler
                exchangeHandler?.Close();

                // Show dialog to inform user
                Dialog.Show(
                    Dialog.DialogType.Info,
                    Dialog.ButtonType.Ok,
                    "Spiel zu Ende",
                    "Dein Gegner hat das Spiel verlassen",
                    (Dialog.Result result) =>
                    {
                        // Navigate back to previous page
                        Navigation.NavigateAndClear(new MenuPage());
                    }
                );
            });
        }

        private void HandleReadyState(bool readyToStart)
        {
            // Return if exchange handler does not exist
            if (exchangeHandler == null)
                return;

            if (readyToStart)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Remove all handlers
                    exchangeHandler.OnTimeout -= HandleTimeout;
                    exchangeHandler.OnDisconnect -= HandleDisconnect;
                    exchangeHandler.OnReady -= HandleReadyState;
                    exchangeHandler.OnReadyAck -= HandleReadyState;

                    // Get all basic ship as list
                    var basicShipList = GetBasicShipList();

                    // Get ships from enemy
                    int[] enemyShipData = exchangeHandler.OpponentShipData;

                    // Insert values to ship list
                    for (int i = 0; i < basicShipList.Count; i++)
                    {
                        Ship ship = basicShipList[i];

                        ship.type = (ShipType)enemyShipData[4 * i + 0];
                        ship.orientation = (ShipOrientation)enemyShipData[4 * i + 1];
                        ship.row = enemyShipData[4 * i + 2];
                        ship.col = enemyShipData[4 * i + 3];
                    }

                    // Navigate to Game page
                    Navigation.RegisterPage(
                        new GamePage(exchangeHandler, gameSetting, dragShips, basicShipList)
                    );
                });
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

            if (exchangeHandler != null)
            {
                // Show dialog to inform user
                Dialog.Show(
                    Dialog.DialogType.Info,
                    Dialog.ButtonType.Ok,
                    "Warte auf Gegner",
                    "Warte bis der Gegner fertig ist"
                );

                // Send ready state with all ships
                exchangeHandler.SendReady(friendlyShips);
            }
            else
            {
                PlacementGrid enemyBattleField = new PlacementGrid(gameSetting.BoardSize);

                // Get all basic ship as list
                var basicShipList = GetBasicShipList();

                // Variable to store state whether randomization successful or not
                bool isRandomizedSucceed;
                do isRandomizedSucceed = enemyBattleField.Randomize(
                    basicShipList.Cast<Ship>().ToList()
                );
                while (!isRandomizedSucceed);

                // Navigate to Game page
                Navigation.RegisterPage(new GamePage(gameSetting, dragShips, basicShipList));
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
                Dialog.Show(
                    Dialog.DialogType.Info,
                    Dialog.ButtonType.Ok,
                    "Ups!",
                    "Das hat leider nicht geklappt, versuch es noch mal oder ändere die Anzahl an Verfügbaren Schiffen!"
                );
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
            // Reset
            Reset();

            // Set ship to side bar
            InitShips();
        }

        /// <summary>
        /// Event handler for the Back button click
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The RoutedEventArgs</param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to previous page
            Navigation.NavigateBack();
        }

        /// <summary>
        /// Event handler for the Cancel button click
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The RoutedEventArgs</param>
        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (exchangeHandler != null)
            {
                // Send Disconnect
                await exchangeHandler.SendDisconnect();
                await exchangeHandler.Close();
            }

            // Navigate to Menu page
            Navigation.NavigateAndClear(new MenuPage());
        }
    }
}
