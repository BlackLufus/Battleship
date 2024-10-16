using Battleship.Global;
using System.Diagnostics;
using static Battleship.Playground.Playground;

namespace Battleship.Lobby
{
    public class BattleField(GameSetting gameSetting)
    {
        public enum FieldState
        {
            Restricted = -1,
            Water = 0,
            Ship = 1,
            Marked = 2
        }

        public enum ShotState
        {
            Miss = -1,
            Water = 0,
            Ship = 1,
            Hit = 2,
            Sunk = 3
        }

        private readonly GameSetting gameSetting = gameSetting;
        private int[,] field = new int[gameSetting.FieldSize, gameSetting.FieldSize];
        public int[,] Field { get { return field; } }
        public int[,] FieldNoRestiction
        {
            get
            {
                for (int i = 0; i < gameSetting.FieldSize; i++)
                {
                    for (int j = 0; j < gameSetting.FieldSize; j++)
                    {
                        if (field[i, j] == (int)FieldState.Restricted)
                        {
                            field[i, j] = (int)FieldState.Water;
                        }
                    }
                }
                return field;
            }
        }

        private Ship? lastShip;

        public static bool IsShipOnPosition(Ship ship, int row, int column)
        {
            if (ship.shipOrientation == Ship.ShipOrientation.Vertical)
            {
                if (row < ship.row || row >= ship.row + (int)ship.shipType || column != ship.column)
                {
                    return false;
                }
            }
            else
            {
                if (column < ship.column || column >= ship.column + (int)ship.shipType || row != ship.row)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CheckShip(int row, int column, Ship.ShipType shipType, Ship.ShipOrientation shipOrientation, FieldState checkForFieldState)
        {
            if (row < 0 || column < 0 || row >= gameSetting.FieldSize || column >= gameSetting.FieldSize)
            {
                return false;
            }
            if (shipOrientation == Ship.ShipOrientation.Vertical)
            {
                for (int i = 0; i < (int)shipType; i++)
                {
                    if (row + i >= gameSetting.FieldSize || field[row + i, column] != (int)checkForFieldState)
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < (int)shipType; i++)
                {
                    if (column + i >= gameSetting.FieldSize || field[row, column + i] != (int)checkForFieldState)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool SetShip(int row, int column, Ship.ShipType shipType, Ship.ShipOrientation shipOrientation, FieldState? fieldState)
        {   
            if (CheckShip(row, column, shipType, shipOrientation, fieldState == null ? FieldState.Water : (fieldState == FieldState.Ship ? FieldState.Marked : FieldState.Water)))
            {
                fieldState ??= FieldState.Ship;
                if (fieldState == FieldState.Marked)
                {
                    lastShip = new Ship(row, column, shipType, shipOrientation);
                }
                if (shipOrientation == Ship.ShipOrientation.Vertical)
                {
                    for (int i = 0; i < (int)shipType; i++)
                    {
                        field[row + i, column] = (int)fieldState;
                        AddRestrictedArea(row + i, column);
                    }
                }
                else
                {
                    for (int i = 0; i < (int)shipType; i++)
                    {
                        field[row, column + i] = (int)fieldState;
                        AddRestrictedArea(row, column + i);
                    }
                }
                return true;
            }
            return false;
        }

        public bool Randomize(List<Ship> shipList)
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

                if (SetShip(row, column, shipList[index].shipType, orientation, null))
                {
                    shipList[index].row = row;
                    shipList[index].column = column;
                    shipList[index].shipOrientation = orientation;
                    iteration = 0;
                    index++;
                }
                else if (resetIteration > maxResetIteration)
                {
                    Debug.WriteLine("Set ships randomly state: FAILED (Iterations: " + totalIterations + " & ResetIteration " + resetIteration + ")");
                    Reset();
                    return false;
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
            return true;
        }

        public bool RemoveShip(int row, int column, Ship.ShipType shipType, Ship.ShipOrientation shipOrientation, FieldState checkForFieldState)
        {
            if (CheckShip(row, column, shipType, shipOrientation, checkForFieldState))
            {
                if (shipOrientation == Ship.ShipOrientation.Vertical)
                {
                    for (int i = 0; i < (int)shipType; i++)
                    {
                        field[row + i, column] = (int)FieldState.Water;
                    }
                }
                else
                {
                    for (int i = 0; i < (int)shipType; i++)
                    {
                        field[row, column + i] = (int)FieldState.Water;
                    }
                }
                if (gameSetting.RestrictedArea)
                {
                    RemoveRestictedArea();
                }
                return true;
            }
            return false;
        }

        public void RemoveLastShip()
        {
            if (lastShip != null)
            {
                RemoveShip(lastShip.row, lastShip.column, lastShip.shipType, lastShip.shipOrientation, FieldState.Marked);
                lastShip = null;
            }
        }

        public void AddRestrictedArea(int row, int column)
        {
            if (row > 0 && field[row - 1, column] == (int)FieldState.Water)
            {
                field[row - 1, column] = (int)FieldState.Restricted;
            }
            if (row < gameSetting.FieldSize - 1 && field[row + 1, column] == (int)FieldState.Water)
            {
                field[row + 1, column] = (int)FieldState.Restricted;
            }
            if (column > 0 && field[row, column - 1] == (int)FieldState.Water)
            {
                field[row, column - 1] = (int)FieldState.Restricted;
            }
            if (column < gameSetting.FieldSize - 1 && field[row, column + 1] == (int)FieldState.Water)
            {
                field[row, column + 1] = (int)FieldState.Restricted;
            }
            if (row > 0 && column > 0 && field[row - 1, column - 1] == (int)FieldState.Water)
            {
                field[row - 1, column - 1] = (int)FieldState.Restricted;
            }
            if (row < gameSetting.FieldSize - 1 && column < gameSetting.FieldSize - 1 && field[row + 1, column + 1] == (int)FieldState.Water)
            {
                field[row + 1, column + 1] = (int)FieldState.Restricted;
            }
            if (row > 0 && column < gameSetting.FieldSize - 1 && field[row - 1, column + 1] == (int)FieldState.Water)
            {
                field[row - 1, column + 1] = (int)FieldState.Restricted;
            }
            if (row < gameSetting.FieldSize - 1 && column > 0 && field[row + 1, column - 1] == (int)FieldState.Water)
            {
                field[row + 1, column - 1] = (int)FieldState.Restricted;
            }
        }

        public void RemoveRestictedArea()
        {
            for (int i = 0; i < gameSetting.FieldSize; i++)
            {
                for (int j = 0; j < gameSetting.FieldSize; j++)
                {
                    if (field[i, j] == (int)FieldState.Restricted)
                    {
                        field[i, j] = (int)FieldState.Water;
                    }
                }
            }
            for (int i = 0; i < gameSetting.FieldSize; i++)
            {
                for (int j = 0; j < gameSetting.FieldSize; j++)
                {
                    if (field[i, j] == (int)FieldState.Ship)
                    {
                        AddRestrictedArea(i, j);
                    }
                }
            }
        }

        public void Reset()
        {
            for (int i = 0; i < gameSetting.FieldSize; i++)
            {
                for (int j = 0; j < gameSetting.FieldSize; j++)
                {
                    field[i, j] = (int)FieldState.Water;
                }
            }
            lastShip = null;

        }   

        public void Dump()
        {
            for (int i = 0; i < gameSetting.FieldSize; i++)
            {
                for (int j = 0; j < gameSetting.FieldSize; j++)
                {
                    Debug.Write(field[i, j] + " ");
                }
                Debug.WriteLine("");
            }
        }
    }
}
