using Battleship.Global;
using System.Diagnostics;
using static Battleship.Global.Ship;
using System.Windows.Controls;
using static Battleship.Playground.Playground;
using System.Windows;
using System.Numerics;
using System;
using System.Drawing;

namespace Battleship.Lobby
{
    // Represents the battlefield where ships are placed
    public class BattleField(int boardSize)
    {
        // The state of each field on the board
        public enum FieldState
        {
            Blocked = -1,
            Water = 0,
            Ship = 1,
            Marked = 2
        }

        public delegate void OnChangedEventHandler();
        public event OnChangedEventHandler? OnChangedEvent;

        private readonly int boardSize = boardSize;

        // The board representation: 0 = free, >0 = blocked
        private int[,] board = new int[boardSize, boardSize];

        // The list of ships on the board
        public List<Ship> ships = new List<Ship>();

        // The current ship being placed on the board and its validity
        public Ship? currentShip = null;

        /// <summary>
        /// Get ship at position or create new ship if none exists and remove it from the field if found
        /// </summary>
        /// <param name="row">The row of the ship</param>
        /// <param name="col">The column of the ship</param>
        /// <param name="type">The type of the ship</param>
        /// <param name="orientation">The orientation of the ship</param>
        /// <returns>The ship at the position or a new ship</returns>
        public Ship Get(int row, int col, ShipType type, ShipOrientation orientation)
        {
            foreach (Ship ship in ships)
            {
                if (ship.HasPosition(row, col))
                {
                    Ship currentShip = ship;
                    BlockSurroundingCells(ship, false);
                    ships.Remove(ship);
                    return ship;
                }
            }
            return new Ship(row, col, type, orientation);
        }

        /// <summary>
        /// Mark a position for the current ship
        /// </summary>
        /// <param name="row">The row of the ship</param>
        /// <param name="col">The column of the ship</param>
        /// <param name="type">The type of the ship</param>
        /// <param name="orientation">The orientation of the ship</param>
        /// <returns>True if the position is valid, otherwise false</returns>
        public bool Mark(int row, int col, ShipType type, ShipOrientation orientation)
        {
            if (currentShip == null)
            {
                currentShip = Get(row, col, type, orientation);
                OnChangedEvent?.Invoke();
            }

            bool dragShipValuesChanged = false;

            if (currentShip.row != row || currentShip.column != col || currentShip.shipOrientation != orientation)
                dragShipValuesChanged = true;

            currentShip.row = row;
            currentShip.column = col;
            currentShip.shipOrientation = orientation;

            bool isValidPosition = IsValidPosition(currentShip);

            if (dragShipValuesChanged)
                OnChangedEvent?.Invoke();

            return isValidPosition;
        }

        /// <summary>
        /// Add the current ship to the field
        /// </summary>
        /// <returns>True if the ship was placed successfully, otherwise false</returns>
        public bool Add()
        {
            if (currentShip == null)
                return false;

            if (!IsValidPosition(currentShip))
            {
                currentShip = null;
                return false;
            }
            else
            {
                BlockSurroundingCells(currentShip);
                ships.Add(currentShip);
                currentShip = null;
                OnChangedEvent?.Invoke();
                return true;
            }
        }

        public void Remove()
        {
            currentShip = null;
            OnChangedEvent?.Invoke();
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
        public List<Ship>? Randomize(List<Ship> shipList)
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
                iteration++;
                totalIterations++;
                int row = randomRow.Next(0, boardSize);
                int col = randomColumn.Next(0, boardSize);
                Ship.ShipOrientation orientation = randomOrientation.Next(0, 2) == 0 ? Ship.ShipOrientation.Horizontal : Ship.ShipOrientation.Vertical;

                Ship tmp = new Ship(row, col, shipList[index].shipType, orientation);

                if (IsValidPosition(tmp))
                {
                    BlockSurroundingCells(tmp);
                    ships.Add(tmp);
                    iteration = 0;
                    index++;
                }
                else if (totalEpisodes > maxTotalEpisodes)
                {
                    Debug.WriteLine("Set ships randomly state: FAILED (Iterations: " + totalIterations + " & Episodes " + totalEpisodes + ")");
                    Reset();
                    return null;
                }
                else if (iteration > maxIteration)
                {
                    Reset();
                    index = 0;
                    iteration = 0;
                    totalEpisodes++;
                }
            }
            Debug.WriteLine("Set ships randomly state: SUCCESSFUL (Iterations: " + totalIterations + " & Episodes " + totalEpisodes + ")");
            return ships;
        }

        /// <summary>
        /// Check if the ship can be placed at the given position
        /// </summary>
        /// <param name="ship">The ship to check</param>
        /// <returns>True if the position is valid, otherwise false</returns>
        public bool IsValidPosition(Ship ship)
        {
            int len = (int)ship.shipType;
            // Horizontal
            if (ship.shipOrientation == Ship.ShipOrientation.Horizontal)
            {
                for (int i = 0; i < len; i++)
                {
                    int r = ship.row;
                    int c = ship.column + i;

                    // If cell is not on the board at all
                    if (r < 0 || r >= boardSize ||
                        c < 0 || c >= boardSize)
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
                int c = ship.column;

                if (r < 0 || r >= boardSize ||
                    c < 0 || c >= boardSize)
                    return false;

                if (board[r, c] > 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Block or unblock surrounding cells of a ship
        /// </summary>
        /// <param name="ship">The ship to block surrounding cells for</param>
        /// <param name="add">True to block, false to unblock</param>
        public void BlockSurroundingCells(Ship ship, bool add = true)
        {
            int len = (int)ship.shipType;
            int delta = add ? 1 : -1;

            if (ship.shipOrientation == ShipOrientation.Horizontal)
            {
                for (int i = -1; i <= len; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        int r = ship.row + j;
                        int c = ship.column + i;

                        if (r >= 0 && r < boardSize &&
                            c >= 0 && c < boardSize)
                        {
                            board[r, c] += delta;
                        }
                    }
                }
            }
            else
            {
                for (int i = -1; i <= len; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        int r = ship.row + i;
                        int c = ship.column + j;

                        if (r >= 0 && r < boardSize &&
                            c >= 0 && c < boardSize)
                        {
                            board[r, c] += delta;
                        }
                    }
                }
            }
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
                if (currentShip.shipOrientation == Ship.ShipOrientation.Horizontal)
                {
                    for (int i = 0; i < (int)currentShip.shipType; i++)
                    {
                        boardState[currentShip.row, currentShip.column + i] = FieldState.Marked;
                    }
                }
                else
                {
                    for (int i = 0; i < (int)currentShip.shipType; i++)
                    {
                        boardState[currentShip.row + i, currentShip.column] = FieldState.Marked;
                    }
                }
            }
            foreach (Ship ship in ships)
            {
                if (ship.shipOrientation == Ship.ShipOrientation.Horizontal)
                {
                    for (int i = 0; i < (int)ship.shipType; i++)
                    {
                        boardState[ship.row, ship.column + i] = FieldState.Ship;
                    }
                }
                else
                {
                    for (int i = 0; i < (int)ship.shipType; i++)
                    {
                        boardState[ship.row + i, ship.column] = FieldState.Ship;
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
