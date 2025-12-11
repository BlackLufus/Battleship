using Battleship.Global;
using System.Diagnostics;
using static Battleship.Global.Ship;
using System.Windows.Controls;
using static Battleship.Playground.Playground;

namespace Battleship.Lobby
{
    public class BattleField(GameSetting gameSetting)
    {
        public enum FieldState
        {
            Blocked = -1,
            Water = 0,
            Ship = 1,
            Marked = 2
        }

        private readonly GameSetting gameSetting = gameSetting;
        private int[,] field = new int[gameSetting.FieldSize, gameSetting.FieldSize];
        public int[,] FieldNoRestiction
        {
            get
            {
                for (int i = 0; i < gameSetting.FieldSize; i++)
                {
                    for (int j = 0; j < gameSetting.FieldSize; j++)
                    {
                        if (field[i, j] == (int)FieldState.Blocked)
                        {
                            field[i, j] = (int)FieldState.Water;
                        }
                    }
                }
                return field;
            }
        }


        private int[,] board = new int[gameSetting.FieldSize, gameSetting.FieldSize];
        public List<Ship> ships = new List<Ship>();
        public Ship? markedShip = null;
        public bool isNewShip = false;

        public Ship GetShip(int row, int column, Ship.ShipType shipType, Ship.ShipOrientation shipOrientation)
        {
            if (!isNewShip)
            {
                foreach (Ship ship in ships)
                {
                    if (ship.row == row && ship.column == column)
                    {
                        Debug.WriteLine("Existing Ship Retrieved");
                        Ship retrievedShip = ship;
                        BlockSurroundingCells(ship, false);
                        ships.Remove(ship);
                        return retrievedShip;
                    }
                }
            }
            return new Ship(row, column, shipType, shipOrientation);
        }

        /**
         * @Description Mark the board with a ship
         * @Param row The row where the ship should be placed
         * @Param column The column where the ship should be placed
         * @Param shipType The type of the ship
         * @Param shipOrientation The orientation of the ship
         * @Return true if the ship was placed successfully, otherwise false
         */
        public bool Mark(int row, int column, Ship.ShipType shipType, Ship.ShipOrientation shipOrientation)
        {
            Ship ship = GetShip(row, column, shipType, shipOrientation);
            if (!IsBlocked(ship))
            {
                markedShip = ship;
                return true;
            }
            return false;
        }

        /**
         * @Description Add a ship the board
         * @Return true if the ship was placed successfully, otherwise false
         */
        public Ship? Add()
        {
            isNewShip = false;
            if (markedShip != null)
            {
                ships.Add(markedShip);
                BlockSurroundingCells(markedShip);
                markedShip = null;
                return ships.Last();
            }
            markedShip = null;
            return null;
        }

        /**
         * @Description Remove a ship the board
         * @Return true if the ship was placed successfully, otherwise false
         */
        public bool Remove()
        {
            if (markedShip != null)
            {
                markedShip = null;
                return true;
            }
            return false;
        }

        public bool RemoveLast()
        {
            if (markedShip != null)
            {
                markedShip = null;
                return true;
            }
            return false;
        }

        /**
         * @Description Remove all ships from the board
         * @Return true if the ship was placed successfully, otherwise false
         */
        public void Reset()
        {
            board = new int[gameSetting.FieldSize, gameSetting.FieldSize];
            ships.Clear();
        }

        public List<Ship> Randomize(List<Ship> shipList)
        {
            int index = 0;

            Random randomRow = new();
            Random randomColumn = new();
            Random randomOrientation = new();

            int totalIterations = 0;
            int iteration = 0;
            int maxIteration = 1000;
            int resetIteration = 0;
            int maxResetIteration = 250;

            while (shipList.Count > index)
            {
                iteration++;
                totalIterations++;
                int row = randomRow.Next(0, gameSetting.FieldSize);
                int column = randomColumn.Next(0, gameSetting.FieldSize);
                Ship.ShipOrientation orientation = randomOrientation.Next(0, 2) == 0 ? Ship.ShipOrientation.Horizontal : Ship.ShipOrientation.Vertical;

                isNewShip = true;
                if (Mark(row, column, shipList[index].shipType, orientation))
                {
                    Add();
                    iteration = 0;
                    index++;
                }
                else if (resetIteration > maxResetIteration)
                {
                    Debug.WriteLine("Set ships randomly state: FAILED (Iterations: " + totalIterations + " & ResetIteration " + resetIteration + ")");
                    Reset();
                    return null;
                }
                else if (iteration > maxIteration)
                {
                    Reset();
                    index = 0;
                    iteration = 0;
                    resetIteration++;
                }
            }
            Debug.WriteLine("Set ships randomly state: SUCCESSFUL (Iterations: " + totalIterations + " & ResetIteration " + resetIteration + ")");
            return ships;
        }

        public bool IsBlocked(Ship ship)
        {
            int len = (int)ship.shipType;
            if (ship.shipOrientation == Ship.ShipOrientation.Horizontal)
            {
                if (ship.column < 0 || ship.column + len > gameSetting.FieldSize)
                {
                    return true;
                }
                for (int i = 0; i < len; i++)
                {
                    if (board[ship.row, ship.column + i] > 0)
                    {
                        return true;
                    }
                }
            }
            else if (ship.shipOrientation == Ship.ShipOrientation.Vertical)
            {
                if (ship.row < 0 || ship.row + len > gameSetting.FieldSize)
                {
                    return true;
                }
                for (int i = 0; i < len; i++)
                {
                    if (board[ship.row + i, ship.column] > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void BlockSurroundingCells(Ship ship, bool add = true)
        {
            int len = (int)ship.shipType;
            if (ship.shipOrientation == Ship.ShipOrientation.Horizontal)
            {
                for (int i = -1; i <= len; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        int row = ship.row + j;
                        int column = ship.column + i;
                        if (row >= 0 && row < gameSetting.FieldSize && column >= 0 && column < gameSetting.FieldSize)
                        {
                            board[row, column] += add ? 1 : -1;
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
                        int row = ship.row + i;
                        int column = ship.column + j;
                        if (row >= 0 && row < gameSetting.FieldSize && column >= 0 && column < gameSetting.FieldSize)
                        {
                            board[row, column] += add ? 1 : -1;
                        }
                    }
                }
            }
        }

        public FieldState[,] GetBoardState()
        {
            FieldState[,] boardState = new FieldState[gameSetting.FieldSize, gameSetting.FieldSize];
            for (int i = 0; i < gameSetting.FieldSize; i++)
            {
                for (int j = 0; j < gameSetting.FieldSize; j++)
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
            if (markedShip != null)
            {
                if (markedShip.shipOrientation == Ship.ShipOrientation.Horizontal)
                {
                    for (int i = 0; i < (int)markedShip.shipType; i++)
                    {
                        boardState[markedShip.row, markedShip.column + i] = FieldState.Marked;
                    }
                }
                else
                {
                    for (int i = 0; i < (int)markedShip.shipType; i++)
                    {
                        boardState[markedShip.row + i, markedShip.column] = FieldState.Marked;
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
            for (int i = 0; i < gameSetting.FieldSize; i++)
            {
                for (int j = 0; j < gameSetting.FieldSize; j++)
                {
                    Debug.Write(board[i, j] + " ");
                }
                Debug.WriteLine("");
            }
        }
    }
}
