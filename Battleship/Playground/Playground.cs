using Battleship.Lobby;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Playground
{
    public class Playground(int[,] field, List<Ship> ships)
    {
        public enum ShotResult
        {
            Miss = -1,
            None = 0,
            Hit = 2,
            Sunk = 3
        }

        public enum FieldState
        {
            Restrict = -2,
            Miss = -1,
            Water = 0,
            Ship = 1,
            Hit = 2,
            Sunk = 3
        }

        private readonly int[,] field = field;
        public int[,] Field { get { return field; } }
        private readonly List<Ship> ships = ships;
        public List<Ship> Ships { get { return ships; } }

        public ShotResult Shot(int row, int col)
        {
            if (field[row, col] == (int)FieldState.Restrict || field[row, col] == (int)FieldState.Miss || field[row, col] == (int)FieldState.Hit || field[row, col] == (int)FieldState.Sunk)
            {
                return ShotResult.None;
            }
            else if (field[row, col] == (int)FieldState.Water)
            {
                field[row, col] = (int)ShotResult.Miss;
                return ShotResult.Miss;
            }
            else
            {
                field[row, col] = (int)ShotResult.Hit;
                if (IsSunk(row, col))
                {
                    return ShotResult.Sunk;
                }
                else
                {
                    return ShotResult.Hit;
                }
            }
        }

        public bool WasShot(int row, int col)
        {
            return field[row, col] != (int)FieldState.Water && field[row, col] != (int)FieldState.Ship;
        }

        private bool IsSunk(int row, int col)
        {
            foreach (Ship ship in ships)
            {
                if (ship.IsHit(row, col) && ship.IsSunk)
                {
                    MarkRestictedArea(ship);
                    return true;
                }
            }
            return false;
        }

        private void MarkRestictedArea(Ship ship)
        {
            if (ship.shipOrientation == Ship.ShipOrientation.Horizontal)
            {
                for (int row = ship.row - 1; row <= ship.row + 1; row++)
                {
                    for (int col = ship.column - 1; col <= ship.column + (int)ship.shipType; col++)
                    {
                        if (row >= 0 && row < field.GetLength(0) && col >= 0 && col < field.GetLength(1) && field[row, col] == (int)FieldState.Water)
                        {
                            field[row, col] = (int)FieldState.Restrict;
                        }
                    }
                }
            }
            else
            {
                for (int row = ship.row - 1; row <= ship.row + (int)ship.shipType; row++)
                {
                    for (int col = ship.column - 1; col <= ship.column + 1; col++)
                    {
                        if (row >= 0 && row < field.GetLength(0) && col >= 0 && col < field.GetLength(1) && field[row, col] == (int)FieldState.Water)
                        {
                            field[row, col] = (int)FieldState.Restrict;
                        }
                    }
                }
            }
        }

        public bool IsAllSunk()
        {
            return ships.All(ship => ship.IsSunk);
        }

        public void Dump()
        {
            for (int row = 0; row < field.GetLength(0); row++)
            {
                for (int col = 0; col < field.GetLength(1); col++)
                {
                    Debug.Write((field[row, col].ToString().Length == 2 ? field[row, col] : " " + field[row, col]) + " ");
                }
                Debug.WriteLine("");
            }
        }
    }
}
