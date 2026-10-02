using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Battleship.Core.BattelStrategy;
using Battleship.Core.BattelStrategy.Modes;
using Battleship.Core.Game;
using Battleship.Core.Global;
using Battleship.Core.Network;
using Battleship.UI.Lobby;
using Battleship.UI.Navi;
using Battleship.UI.Resources.Components;
using Battleship.UI.Resources.userControls;

namespace Battleship.UI.Game
{
    /// <summary>
    /// Interaktionslogik für GamePage.xaml
    /// </summary>
    public partial class GamePage : Page
    {
        private readonly GameLogic gameLogic;

        private readonly GameSetting gameSetting;
        private readonly PlaygroundBoardLogic playerBoard;
        private readonly PlaygroundBoardLogic opponentBoard;

        private readonly StackPanel[,] playerCellPanel;
        private readonly CellState[,] playerCellState;
        private readonly StackPanel[,] opponentCellPanel;
        private readonly CellState[,] opponentCellState;

        private readonly ImageSource WaterImage = new BitmapImage(
            new Uri("pack://application:,,,/Resources/Images/water-field.png")
        );

        private readonly ImageSource FireImage = new BitmapImage(
            new Uri("pack://application:,,,/Resources/Images/fire.png")
        );

        private readonly ImageSource RedCrossImage = new BitmapImage(
            new Uri("pack://application:,,,/Resources/Images/red-cross.png")
        );

        private readonly ImageSource RestrictionImage = new BitmapImage(
            new Uri("pack://application:,,,/Resources/Images/restriction.png")
        );

        private readonly ImageSource TargetImage = new BitmapImage(
            new Uri("pack://application:,,,/Resources/Images/target.png")
        );

        public GamePage(
            GameSetting gameSetting,
            List<DragShip> playerDragShips,
            List<DragShip> opponentDragShips
        )
        {
            this.gameSetting = gameSetting;
            playerCellPanel = new StackPanel[gameSetting.BoardSize, gameSetting.BoardSize];
            playerCellState = new CellState[gameSetting.BoardSize, gameSetting.BoardSize];
            opponentCellPanel = new StackPanel[gameSetting.BoardSize, gameSetting.BoardSize];
            opponentCellState = new CellState[gameSetting.BoardSize, gameSetting.BoardSize];

            InitializeComponent();

            this.playerBoard = new PlaygroundBoardLogic(
                gameSetting.BoardSize,
                [.. playerDragShips.Cast<Ship>()]
            );
            foreach (DragShip ship in playerDragShips)
            {
                if (FriendlyShipCanvas != null)
                    ship.updateCanvas(FriendlyShipCanvas);
                ship.Show();
            }
            this.opponentBoard = new PlaygroundBoardLogic(
                gameSetting.BoardSize,
                [.. opponentDragShips.Cast<Ship>()]
            );
            foreach (DragShip dragShip in opponentDragShips)
            {
                dragShip.updateCanvas(EnemyShipCanvas);
            }

            SetPlayerUsername(Variables.Username);
            SetOpponentUsername($"Computer{new Random().Next(1000, 9999)}");

            InitPlaygroundBoard(
                EnemyWaterGrid,
                EnemyCellGrid,
                EnemyTargetGrid,
                opponentCellPanel,
                false
            );
            InitPlaygroundBoard(
                FriendlyWaterGrid,
                FriendlyCellGrid,
                FriendlyTargetGrid,
                playerCellPanel,
                true
            );

            gameLogic = new GameLogic(gameSetting, opponentBoard, playerBoard);
            gameLogic.OnShotAtOpponentBoard += HandleShotAtOpponentBoard;
            gameLogic.OnShotAtPlayerBoard += HandleShotAtPlayerBoard;
            gameLogic.OnGameEnded += HandleGameEnded;
            gameLogic.StartSinglePlayerMode();

            string whoIsTurn =
                gameLogic.turn == TurnType.Player
                    ? "Du bist am Zug!"
                    : $"{OpponentUsername.Content} ist am Zug!";
            Chat.AddSystemMessage($"Die Schlacht beginnt. Alle Schiffe in Position! {whoIsTurn}");
        }

        public GamePage(
            ExchangeHandler exchangeHandler,
            GameSetting gameSetting,
            List<DragShip> playerDragShips,
            List<DragShip> opponentDragShips
        )
        {
            this.gameSetting = gameSetting;
            playerCellPanel = new StackPanel[gameSetting.BoardSize, gameSetting.BoardSize];
            playerCellState = new CellState[gameSetting.BoardSize, gameSetting.BoardSize];
            opponentCellPanel = new StackPanel[gameSetting.BoardSize, gameSetting.BoardSize];
            opponentCellState = new CellState[gameSetting.BoardSize, gameSetting.BoardSize];

            InitializeComponent();

            this.playerBoard = new PlaygroundBoardLogic(
                gameSetting.BoardSize,
                [.. playerDragShips.Cast<Ship>()]
            );
            foreach (DragShip ship in playerDragShips)
            {
                if (FriendlyShipCanvas != null)
                    ship.updateCanvas(FriendlyShipCanvas);
                ship.Show();
            }
            this.opponentBoard = new PlaygroundBoardLogic(
                gameSetting.BoardSize,
                [.. opponentDragShips.Cast<Ship>()]
            );
            foreach (DragShip dragShip in opponentDragShips)
            {
                dragShip.updateCanvas(EnemyShipCanvas);
            }

            SetPlayerUsername(exchangeHandler.PlayerUsername);
            SetOpponentUsername(exchangeHandler.OpponentUsername!);

            int id = 0;

            Chat.OnSendButtonClick += (string message) =>
            {
                exchangeHandler.SendMessage(id++, message);
                Chat.AddMessage(exchangeHandler.PlayerUsername, message);
            };
            exchangeHandler.OnMessage += (int id, string message) =>
            {
                Chat.AddMessage(exchangeHandler.OpponentUsername!, message);
                Debug.WriteLine("===================================");
                Debug.WriteLine($" >>> Nachricht ({id}): {message}");
                Debug.WriteLine("===================================");
            };
            exchangeHandler.OnMessageAck += (int id) =>
            {
                Debug.WriteLine("===================================");
                Debug.WriteLine($" >>> Nachricht mit der id {id} versendet");
                Debug.WriteLine("===================================");
            };

            InitPlaygroundBoard(
                EnemyWaterGrid,
                EnemyCellGrid,
                EnemyTargetGrid,
                opponentCellPanel,
                false
            );
            InitPlaygroundBoard(
                FriendlyWaterGrid,
                FriendlyCellGrid,
                FriendlyTargetGrid,
                playerCellPanel,
                true
            );

            gameLogic = new GameLogic(exchangeHandler, gameSetting, opponentBoard, playerBoard);
            gameLogic.OnShotAtOpponentBoard += HandleShotAtOpponentBoard;
            gameLogic.OnShotAtPlayerBoard += HandleShotAtPlayerBoard;
            gameLogic.OnGameEnded += HandleGameEnded;
            gameLogic.OnTimeout += HandleTimout;
            gameLogic.OnDisconnect += HandleDisconnet;
            gameLogic.StartMultiPlayerMode();

            string whoIsTurn =
                gameLogic.turn == TurnType.Player
                    ? "Du bist am Zug!"
                    : $"{OpponentUsername.Content} ist am Zug!";
            Chat.AddSystemMessage($"Die Schlacht beginnt. Alle Schiffe in Position! {whoIsTurn}");
        }

        /// <summary>
        /// Sets the opponent username
        /// </summary>
        /// <param name="username">The username to set</param>
        private void SetOpponentUsername(string username)
        {
            OpponentUsername.Content = username;
        }

        /// <summary>
        /// Sets the player username
        /// </summary>
        /// <param name="username">The username to set</param>
        private void SetPlayerUsername(string username)
        {
            PlayerUsername.Content = username;
        }

        /// <summary>
        /// Init the playground either for the friendly or the enemy board
        /// </summary>
        /// <param name="waterGrid">Level 1 grid: (aka water grid)</param>
        /// <param name="CellState">Level 3 grid: (aka cell state</param>
        /// <param name="targetGrid">Level 4 grid: (aka target grid, spawns a target image)</param>
        /// <param name="cellPanel">The array to store each cell</param>
        /// <param name="playerBoard">An indicator to determine whether the input comes from player or opponent.</param>
        private void InitPlaygroundBoard(
            Grid waterGrid,
            Grid CellState,
            Grid targetGrid,
            StackPanel[,] cellPanel,
            bool playerBoard
        )
        {
            // Erstelle 10 Zeilen und 10 Spalten
            for (int i = 0; i < gameSetting.BoardSize; i++)
            {
                waterGrid.RowDefinitions.Add(new RowDefinition());
                waterGrid.ColumnDefinitions.Add(new ColumnDefinition());
                CellState.RowDefinitions.Add(new RowDefinition());
                CellState.ColumnDefinitions.Add(new ColumnDefinition());
                targetGrid.RowDefinitions.Add(new RowDefinition());
                targetGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Füge in jede Zelle ein Bild hinzu
            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize; col++)
                {
                    // Water Grid
                    Image image = new() { Source = WaterImage };

                    // Position in Grid
                    Grid.SetRow(image, row);
                    Grid.SetColumn(image, col);

                    // Add to Grid
                    waterGrid.Children.Add(image);

                    // Cell State Grid
                    StackPanel stackPanel = new StackPanel();
                    cellPanel[row, col] = stackPanel;

                    // Position in Grid
                    Grid.SetRow(stackPanel, row);
                    Grid.SetColumn(stackPanel, col);

                    // Add to Grid
                    CellState.Children.Add(stackPanel);

                    // Target Grid
                    Border cell = new Border()
                    {
                        Tag = new Point(row, col),
                        Background = Brushes.Transparent,
                    };

                    // Add Event listeners
                    cell.MouseEnter += OnMouseEnter;
                    cell.MouseLeave += OnMouseLeave;
                    if (!playerBoard)
                    {
                        cell.PreviewMouseDown += OnPreviewMouseDown;
                        cell.PreviewMouseUp += OnPreviewMouseUp;
                    }

                    // Position in Grid
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);

                    // Add to Grid
                    targetGrid.Children.Add(cell);
                }
            }
        }

        /// <summary>
        /// Handle mouse enter event
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The arguments of the mouse event</param>
        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            // The cell triggered by the event
            Border cell = (Border)sender;

            // Add target image to the specific cell
            var image = new Image { Source = TargetImage, Stretch = Stretch.Uniform };
            cell.Child = image;
        }

        /// <summary>
        /// Handle mouse leave event
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The arguments of the mouse event</param>
        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            // The cell triggered by the event
            Border cell = (Border)sender;

            // Change background to transparent
            cell.Background = Brushes.Transparent;
            cell.Child = null;
        }

        /// <summary>
        /// Handle preview mouse down
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The arguments of the mouse button event</param>
        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // The cell triggered by the event
            Border cell = (Border)sender;

            // Get stored state (row, col)
            Point p = (Point)cell.Tag;

            // Shoot at the specific position
            gameLogic.Shoot((int)p.X, (int)p.Y);

            // Change background
            cell.Background = new BrushConverter().ConvertFrom("#44FFFFFF") as Brush;
        }

        /// <summary>
        /// Handle preview mouse up event
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The arguments of the mouse button event</param>
        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // The cell triggered by the event
            Border cell = (Border)sender;

            // Change background to transparent
            cell.Background = Brushes.Transparent;
        }

        /// <summary>
        /// Update a specific cell to a new state
        /// </summary>
        /// <param name="element">The element to update</param>
        /// <param name="state">The new state of the element</param>
        private void UpdateCell(StackPanel element, CellState state)
        {
            if (state == CellState.HIT || state == CellState.SUNK)
            {
                Image image = new() { Source = FireImage };
                element.Children.Add(image);
            }
            else if (state == CellState.MISS)
            {
                Image image = new() { Source = RedCrossImage };
                element.Children.Add(image);
            }
            else if (state == CellState.BLOCKED && gameSetting.RestrictedArea)
            {
                Image image = new() { Source = RestrictionImage };
                element.Children.Add(image);
            }
        }

        /// <summary>
        /// Update the UI either the enemy or the friendly board
        /// </summary>
        /// <param name="newCellState">The new board state</param>
        /// <param name="currentCellState">The current state</param>
        /// <param name="cellPanels">The UI element</param>
        private void UpdateUI(
            CellState[,] newCellState,
            CellState[,] currentCellState,
            StackPanel[,] cellPanels
        )
        {
            for (int x = 0; x < gameSetting.BoardSize; x++)
            {
                for (int y = 0; y < gameSetting.BoardSize; y++)
                {
                    // Update content of a specific Cell
                    if (currentCellState[x, y] != newCellState[x, y])
                    {
                        currentCellState[x, y] = newCellState[x, y];
                        UpdateCell(cellPanels[x, y], newCellState[x, y]);
                    }
                }
            }
        }

        /// <summary>
        /// Handle shot on opponent board event
        /// </summary>
        private void HandleShotAtOpponentBoard()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (gameLogic.turn == TurnType.Opponent)
                    Chat.AddSystemMessage($"{OpponentUsername.Content} ist am Zug!");
                CellState[,] newCellState = opponentBoard.GetBoardState();
                UpdateUI(newCellState, opponentCellState, opponentCellPanel);
            });
        }

        /// <summary>
        /// Handle shot on players board event
        /// </summary>
        private void HandleShotAtPlayerBoard()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (gameLogic.turn == TurnType.Player)
                    Chat.AddSystemMessage("Du bist am Zug!");
                CellState[,] newCellState = playerBoard.GetBoardState();
                UpdateUI(newCellState, playerCellState, playerCellPanel);
            });
        }

        /// <summary>
        /// Handle game ended event
        /// </summary>
        /// <param name="hasWon">An indicator that shows whether the user has won or lost.</param>
        private void HandleGameEnded(bool hasWon)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string title = "Das Spiel ist zu Ende";
                string content = hasWon
                    ? $"Herzlichen Glückwunsch {PlayerUsername.Content}!\nDu hast das Spiel gewonnen."
                    : $"Leider Verloren!\n{OpponentUsername.Content} hat das Spiel gewonnen.";
                Chat.AddSystemMessage(content.Replace("\n", " "));
                Dialog.Show(
                    Dialog.DialogType.Info,
                    Dialog.ButtonType.Ok,
                    title,
                    content,
                    (Dialog.Result result) =>
                    {
                        Debug.WriteLine($"Result clicked: {result}");

                        // Navigate Menu page
                        Navigation.RegisterPage(new LobbyMainPage(false));
                    }
                );
            });
        }

        /// <summary>
        /// Handle timeout event
        /// </summary>
        private void HandleTimout()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Chat.AddSystemMessage("Dein Gegner antwortet nicht mehr.");
                // Show dialog to inform user
                Dialog.Show(
                    Dialog.DialogType.Info,
                    Dialog.ButtonType.Ok,
                    "Timeout",
                    "Dein Gegner antwortet nicht mehr.",
                    (Dialog.Result result) =>
                    {
                        // Navigate Menu page
                        Navigation.RegisterPage(new LobbyMainPage(false));
                    }
                );
            });
        }

        /// <summary>
        /// Handle disconnect event
        /// </summary>
        private void HandleDisconnet()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Chat.AddSystemMessage("Dein Gegner hat das Spiel verlassen.");

                // Show dialog to inform user
                Dialog.Show(
                    Dialog.DialogType.Info,
                    Dialog.ButtonType.Ok,
                    "Spiel zu Ende",
                    "Dein Gegner hat das Spiel verlassen",
                    (Dialog.Result result) =>
                    {
                        // Navigate Menu page
                        Navigation.RegisterPage(new LobbyMainPage(false));
                    }
                );
            });
        }
    }
}
