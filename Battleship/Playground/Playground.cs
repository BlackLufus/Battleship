using Battleship.Global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Playground
{
    public class Playground(int boardSize, List<Ship> ships)
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

        private readonly int[,] field = new int[boardSize, boardSize];
        public int[,] Field { get { return field; } }
        private readonly List<Ship> ships = ships;
        public List<Ship> Ships { get { return ships; } }

        public ShotResult Shoot(int row, int col, bool restrictedArea = true)
        {
            if (WasShot(row, col))
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
                if (IsSunk(row, col, restrictedArea))
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

        private bool IsSunk(int row, int col, bool restrictedArea = true)
        {
            foreach (Ship ship in ships)
            {
                if (ship.Hit(row, col) && ship.IsSunk)
                {
                    if (restrictedArea) MarkRestictedArea(ship);
                    return true;
                }
            }
            return false;
        }

        private void MarkRestictedArea(Ship ship)
        {
            if (ship.orientation == Ship.ShipOrientation.Horizontal)
            {
                for (int row = ship.row - 1; row <= ship.row + 1; row++)
                {
                    for (int col = ship.col - 1; col <= ship.col + (int)ship.type; col++)
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
                for (int row = ship.row - 1; row <= ship.row + (int)ship.type; row++)
                {
                    for (int col = ship.col - 1; col <= ship.col + 1; col++)
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

        public int DeterminedSmallestShipSize()
        {
            return ships.Where(ship => !ship.IsSunk).Min(ship => (int)ship.type);
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

        // Defines a ship on the field if the ships were not defined before
        public void DefineShip(int row, int col, bool isSunk = false)
        {
            field[row, col] = (int)FieldState.Ship;
            // Erstelle ein neues Schiff an der gegebenen Position
            Ship newShip = new(row, col, Ship.ShipType.Carrier, Ship.ShipOrientation.Horizontal);
            if (!isSunk)
            {
                newShip.Shots.Add((row, col));
            }

            // Liste, um gemergte Schiffe zu speichern
            List<Ship> oldShips = new();

            // Iteriere über alle vorhandenen Schiffe
            foreach (Ship ship in ships)
            {
                // Prüfe alle benachbarten Positionen (oben, unten, links, rechts)
                if (ship.Shots.Contains((row - 1, col)))
                {
                    Debug.WriteLine("Merge Ship with TOP ship");
                    newShip.row = Math.Min(newShip.row, ship.row);
                    newShip.type = (Ship.ShipType)((int)ship.type + (int)newShip.type);
                    newShip.Shots.AddRange(ship.Shots);
                    newShip.orientation = Ship.ShipOrientation.Vertical;
                    oldShips.Add(ship);
                }
                if (ship.Shots.Contains((row + 1, col)))
                {
                    Debug.WriteLine("Merge Ship with BOTTOM ship");
                    newShip.type = (Ship.ShipType)((int)ship.type + (int)newShip.type);
                    newShip.Shots.AddRange(ship.Shots);
                    newShip.orientation = Ship.ShipOrientation.Vertical;
                    oldShips.Add(ship);
                }
                if (ship.Shots.Contains((row, col - 1)))
                {
                    Debug.WriteLine("Merge Ship with LEFT ship");
                    newShip.col = Math.Min(newShip.col, ship.col);
                    newShip.type = (Ship.ShipType)((int)ship.type + (int)newShip.type);
                    newShip.orientation = Ship.ShipOrientation.Horizontal;
                    newShip.Shots.AddRange(ship.Shots);
                    oldShips.Add(ship);
                }
                if (ship.Shots.Contains((row, col + 1)))
                {
                    Debug.WriteLine("Merge Ship with RIGHT ship");
                    newShip.type = (Ship.ShipType)((int)ship.type + (int)newShip.type);
                    newShip.orientation = Ship.ShipOrientation.Horizontal;
                    newShip.Shots.AddRange(ship.Shots);
                    oldShips.Add(ship);
                }
            }

            // Entferne alle alten Schiffe, die gemergt wurden
            foreach (Ship ship in oldShips)
            {
                ships.Remove(ship);
            }

            // Füge das neue, gemergte Schiff zur Liste hinzu
            ships.Add(newShip);

            // Debug-Ausgaben zur Überprüfung der Schiffsliste
            Debug.WriteLine("Ships -> " + ships.Count);
            foreach (Ship ship in ships)
            {
                Debug.WriteLine("Ship -> " + ship.type + ", Positionen: " + string.Join(", ", ship.Shots));
            }
        }
    }
}
