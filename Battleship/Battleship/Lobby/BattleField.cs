using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Lobby
{
    public class BattleField
    {
        public enum FieldState
        {
            Restricted = -1,
            Water = 0,
            Ship = 1,
            Marked = 2
        }

        public int size;
        public int[,] field;
        private bool restricted;

        private Ship? lastShip;

        public BattleField(int size, bool restricted)
        {
            this.size = size;
            this.field = new int[size, size];
            this.restricted = restricted;
        }

        public bool CheckShip(int row, int column, Ship.ShipType shipType, Ship.ShipOrientation shipOrientation, FieldState checkForFieldState)
        {
            if (row < 0 || column < 0 || row >= size || column >= size)
            {
                return false;
            }
            if (shipOrientation == Ship.ShipOrientation.Vertical)
            {
                for (int i = 0; i < (int)shipType; i++)
                {
                    if (row + i >= size || field[row + i, column] != (int)checkForFieldState)
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < (int)shipType; i++)
                {
                    if (column + i >= size || field[row, column + i] != (int)checkForFieldState)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool SetShip(int row, int column, Ship.ShipType shipType, Ship.ShipOrientation shipOrientation, FieldState fieldState)
        {
            Debug.WriteLine("SetShip: " + row + " " + column + " " + shipType + " " + shipOrientation + " " + fieldState);
            if (CheckShip(row, column, shipType, shipOrientation, (fieldState == FieldState.Ship ? FieldState.Marked : FieldState.Water)))
            {
                if (fieldState == FieldState.Marked)
                {
                    lastShip = new Ship(row, column, shipType, shipOrientation);
                }
                if (shipOrientation == Ship.ShipOrientation.Vertical)
                {
                    for (int i = 0; i < (int)shipType; i++)
                    {
                        field[row + i, column] = (int)fieldState;
                        if (restricted)
                        {
                            AddRestrictedArea(row + i, column);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < (int)shipType; i++)
                    {
                        field[row, column + i] = (int)fieldState;
                        if (restricted)
                        {
                            AddRestrictedArea(row, column + i);
                        }
                    }
                }
                return true;
            }
            return false;
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
                if (restricted)
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
                RemoveRestictedArea();
            }
        }

        public void AddRestrictedArea(int row, int column)
        {
            if (row > 0 && field[row - 1, column] == (int)FieldState.Water)
            {
                field[row - 1, column] = (int)FieldState.Restricted;
            }
            if (row < size - 1 && field[row + 1, column] == (int)FieldState.Water)
            {
                field[row + 1, column] = (int)FieldState.Restricted;
            }
            if (column > 0 && field[row, column - 1] == (int)FieldState.Water)
            {
                field[row, column - 1] = (int)FieldState.Restricted;
            }
            if (column < size - 1 && field[row, column + 1] == (int)FieldState.Water)
            {
                field[row, column + 1] = (int)FieldState.Restricted;
            }
            if (row > 0 && column > 0 && field[row - 1, column - 1] == (int)FieldState.Water)
            {
                field[row - 1, column - 1] = (int)FieldState.Restricted;
            }
            if (row < size - 1 && column < size - 1 && field[row + 1, column + 1] == (int)FieldState.Water)
            {
                field[row + 1, column + 1] = (int)FieldState.Restricted;
            }
            if (row > 0 && column < size - 1 && field[row - 1, column + 1] == (int)FieldState.Water)
            {
                field[row - 1, column + 1] = (int)FieldState.Restricted;
            }
            if (row < size - 1 && column > 0 && field[row + 1, column - 1] == (int)FieldState.Water)
            {
                field[row + 1, column - 1] = (int)FieldState.Restricted;
            }
        }

        public void RemoveRestictedArea()
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (field[i, j] == (int)FieldState.Restricted)
                    {
                        field[i, j] = (int)FieldState.Water;
                    }
                }
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (field[i, j] == (int)FieldState.Ship)
                    {
                        AddRestrictedArea(i, j);
                    }
                }
            }
        }
    }
}
