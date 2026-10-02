using System.Windows;

namespace Battleship.Core.Game
{
    public enum CellState
    {
        BLOCKED = -2,
        MISS = -1,
        WATER = 0,
        SHIP = 1,
        HIT = 2,
        SUNK = 3,
    }

    public enum ShotResult
    {
        Miss = -1,
        None = 0,
        Hit = 2,
        Sunk = 3,
    }

    public class PlaygroundBoardLogic(int boardSize, List<Ship> ships)
    {
        private readonly int boardSize = boardSize;
        private readonly List<Ship> ships = ships;
        public List<Ship> Ships
        {
            get { return ships; }
        }

        private readonly List<(int x, int y)> shots = [];
        private readonly int[,] board = new int[boardSize, boardSize];

        /// <summary>
        /// Handle shoot at a specific position
        /// </summary>
        /// <param name="row">Position at row</param>
        /// <param name="col">Position at col</param>
        /// <returns>Returns the shot result depending whether the ship was hit or not</returns>
        public ShotResult Shoot(int row, int col)
        {
            // Return None if row and/or col is outside of the board
            if (row < 0 || row >= boardSize || col < 0 || col >= boardSize)
                return ShotResult.None;

            // Return None when the specific position is blocked or already been shot
            if (board[row, col] > 0 || shots.Contains((row, col)))
                return ShotResult.None;

            // Otherwise add the new position to shots on the board
            shots.Add((row, col));

            // Iterate throught each ship to check if a ship was hit
            foreach (Ship ship in ships)
            {
                // Return Hit when a ship was damaged
                if (ship.Hit(row, col))
                {
                    // Return Sunk when a ship has been completely destroyed
                    if (ship.IsSunk)
                    {
                        // Block surroundings cells
                        SurroundingCells.BlockSurroundingCells(board, boardSize, ship);

                        return ShotResult.Sunk;
                    }
                    return ShotResult.Hit;
                }
            }
            // Return Miss when no ship was hit
            return ShotResult.Miss;
        }

        /// <summary>
        /// Check if the position has already been shot.
        /// </summary>
        /// <param name="row">The position in row</param>
        /// <param name="col">The position in col</param>
        /// <returns>Returns true when position was already shot otherwise false</returns>
        public bool CellWasShot(int row, int col)
        {
            if (board[row, col] > 0)
                return true;
            if (shots.Contains((row, col)))
                return true;
            return false;
        }

        /// <summary>
        /// Check if all ships are sunk
        /// </summary>
        /// <returns>Returns true when all ships are sunk otherwise false</returns>
        public bool IsAllSunk()
        {
            return ships.All(ship => ship.IsSunk);
        }

        /// <summary>
        /// Determined smallest ship size
        /// </summary>
        /// <returns>Return the size of the smallest still available ship</returns>
        public int DeterminedSmallestShipSize()
        {
            return ships.Where(ship => !ship.IsSunk).Min(ship => (int)ship.type);
        }

        /// <summary>
        /// Returns the current board state
        /// </summary>
        /// <returns>The board with all states (Blocked/Water/Miss/Hit/Sunk)</returns>
        public CellState[,] GetBoardState()
        {
            CellState[,] boardState = new CellState[boardSize, boardSize];
            // Add all blocked and water informations
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (board[i, j] > 0)
                    {
                        boardState[i, j] = CellState.BLOCKED;
                    }
                    else
                    {
                        boardState[i, j] = CellState.WATER;
                    }
                }
            }
            // Add all Miss informations
            foreach ((int, int) shot in shots)
            {
                boardState[shot.Item1, shot.Item2] = CellState.MISS;
            }
            // Add ships specific informations
            foreach (Ship ship in ships)
            {
                if (ship.orientation == ShipOrientation.Horizontal)
                {
                    for (int i = 0; i < (int)ship.type; i++)
                    {
                        if (ship.IsSunk)
                            boardState[ship.row, ship.col + i] = CellState.SUNK;
                        else if (ship.Shots.Contains((ship.row, ship.col + i)))
                            boardState[ship.row, ship.col + i] = CellState.HIT;
                        else
                            boardState[ship.row, ship.col + i] = CellState.SHIP;
                    }
                }
                else
                {
                    for (int i = 0; i < (int)ship.type; i++)
                    {
                        if (ship.IsSunk)
                            boardState[ship.row + i, ship.col] = CellState.SUNK;
                        else if (ship.Shots.Contains((ship.row + i, ship.col)))
                            boardState[ship.row + i, ship.col] = CellState.HIT;
                        else
                            boardState[ship.row + i, ship.col] = CellState.SHIP;
                    }
                }
            }
            return boardState;
        }
    }
}
