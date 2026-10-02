using System.Diagnostics;

namespace Battleship.Core.Game
{
    // The state of each field on the board
    public enum FieldState
    {
        Blocked = -1,
        Water = 0,
        Ship = 1,
        Marked = 2,
    }

    // Represents the battlefield where ships are placed
    public class PlacementGrid(int boardSize)
    {
        // Event on board state changed
        public delegate void OnStateChangeEventHandler();
        public event OnStateChangeEventHandler? OnStateChangeEvent;

        private readonly int boardSize = boardSize;
        private int[,] board = new int[boardSize, boardSize];

        // The list of ships on the board
        private readonly List<Ship> ships = [];

        // The current ship being placed on the board and its validity
        public Ship? currentShip = null;

        /// <summary>
        /// Mark a position for the current ship
        /// </summary>
        /// <param name="ship">The ship object</param>
        /// <returns>True if the position is valid, otherwise false</returns>
        public bool Mark(Ship ship)
        {
            if (currentShip == null)
            {
                if (ships.Contains(ship))
                {
                    SurroundingCells.BlockSurroundingCells(board, boardSize, ship, false);
                    ships.Remove(ship);
                }
                currentShip = ship;
                OnStateChangeEvent?.Invoke();
            }

            bool isValidPosition = IsValidPosition(currentShip);

            OnStateChangeEvent?.Invoke();

            return isValidPosition;
        }

        /// <summary>
        /// Add the current ship to the field
        /// </summary>
        /// <param name="ship">The ship object</param>
        /// <returns>True if the ship was placed successfully, otherwise false</returns>
        public bool Place(Ship ship)
        {
            if (!IsValidPosition(ship))
            {
                currentShip = null;
                return false;
            }
            else
            {
                SurroundingCells.BlockSurroundingCells(board, boardSize, ship);
                ships.Add(ship);
                currentShip = null;
                OnStateChangeEvent?.Invoke();
                return true;
            }
        }

        /// <summary>
        /// Remove all ships from the board
        /// </summary>
        public void Reset()
        {
            board = new int[boardSize, boardSize];
            ships.Clear();
        }

        /// <summary>
        /// Randomly place ships on the board
        /// </summary>
        /// <param name="shipList">The list of ships to place</param>
        /// <returns>The list of placed ships or null if placement failed</returns>
        public bool Randomize(List<Ship> shipList)
        {
            int index = 0;

            Random randomRow = new();
            Random randomColumn = new();
            Random randomOrientation = new();

            int totalIterations = 0;
            int iteration = 0;
            int maxIteration = 1000;
            int totalEpisodes = 0;
            int maxTotalEpisodes = 250;

            Reset();

            while (shipList.Count > index)
            {
                // Increment for iteration and total iterations
                iteration++;
                totalIterations++;

                // Set Random values for row, col and orientation
                int row = randomRow.Next(0, boardSize);
                int col = randomColumn.Next(0, boardSize);
                ShipOrientation orientation =
                    randomOrientation.Next(0, 2) == 0
                        ? ShipOrientation.Horizontal
                        : ShipOrientation.Vertical;

                // Set attributes for ship
                var ship = shipList[index];
                ship.row = row;
                ship.col = col;
                ship.orientation = orientation;

                // Is ship on a valid position?
                if (IsValidPosition(ship))
                {
                    SurroundingCells.BlockSurroundingCells(board, boardSize, ship);
                    ships.Add(ship);
                    iteration = 0;
                    index++;
                }
                // Check if total episodes exceed max total episodes
                else if (totalEpisodes > maxTotalEpisodes)
                {
                    Debug.WriteLine(
                        "Set ships randomly state: FAILED (Iterations: "
                            + totalIterations
                            + " & Episodes "
                            + totalEpisodes
                            + ")"
                    );
                    Reset();
                    return false;
                }
                // Check if iteration exceed max iterations
                else if (iteration > maxIteration)
                {
                    Reset();
                    index = 0;
                    iteration = 0;
                    totalEpisodes++;
                }
            }
            Debug.WriteLine(
                "Set ships randomly state: SUCCESSFUL (Iterations: "
                    + totalIterations
                    + " & Episodes "
                    + totalEpisodes
                    + ")"
            );
            return true;
        }

        /// <summary>
        /// Check if the ship can be placed at the given position
        /// </summary>
        /// <param name="ship">The ship to check</param>
        /// <returns>True if the position is valid, otherwise false</returns>
        public bool IsValidPosition(Ship ship)
        {
            int len = (int)ship.type;
            // Horizontal
            if (ship.orientation == ShipOrientation.Horizontal)
            {
                for (int i = 0; i < len; i++)
                {
                    int r = ship.row;
                    int c = ship.col + i;

                    // If cell is not on the board at all
                    if (r < 0 || r >= boardSize || c < 0 || c >= boardSize)
                        return false;

                    // If cell is blocked
                    if (board[r, c] > 0)
                        return false;
                }

                return true;
            }
            // Vertical
            for (int i = 0; i < len; i++)
            {
                int r = ship.row + i;
                int c = ship.col;

                if (r < 0 || r >= boardSize || c < 0 || c >= boardSize)
                    return false;

                if (board[r, c] > 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get the current state of the board
        /// </summary>
        /// <returns>The current state of the board</returns>
        public FieldState[,] GetBoardState()
        {
            FieldState[,] boardState = new FieldState[boardSize, boardSize];
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (board[i, j] > 0)
                    {
                        boardState[i, j] = FieldState.Blocked;
                    }
                    else
                    {
                        boardState[i, j] = FieldState.Water;
                    }
                }
            }
            if (currentShip != null && IsValidPosition(currentShip))
            {
                if (currentShip.orientation == ShipOrientation.Horizontal)
                {
                    for (int i = 0; i < (int)currentShip.type; i++)
                    {
                        boardState[currentShip.row, currentShip.col + i] = FieldState.Marked;
                    }
                }
                else
                {
                    for (int i = 0; i < (int)currentShip.type; i++)
                    {
                        boardState[currentShip.row + i, currentShip.col] = FieldState.Marked;
                    }
                }
            }
            foreach (Ship ship in ships)
            {
                if (ship.orientation == ShipOrientation.Horizontal)
                {
                    for (int i = 0; i < (int)ship.type; i++)
                    {
                        boardState[ship.row, ship.col + i] = FieldState.Ship;
                    }
                }
                else
                {
                    for (int i = 0; i < (int)ship.type; i++)
                    {
                        boardState[ship.row + i, ship.col] = FieldState.Ship;
                    }
                }
            }
            return boardState;
        }

        /// <summary>
        /// Dump the field to the console
        /// </summary>
        public void Dump()
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    Debug.Write(board[i, j] + " ");
                }
                Debug.WriteLine("");
            }
        }
    }
}
