using Battleship.Lobby;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static System.Net.Mime.MediaTypeNames;

namespace Battleship.Playground.ComputerLogic
{
    public abstract class GameAILogic(GameSetting gameSetting, Playground playground)
    {
        enum Direction
        {
            Right = 0,
            Up = 1,
            Left = 2,
            Down = 3,
        }

        private readonly GameSetting gameSetting = gameSetting;
        private readonly Playground playground = playground;

        private Shot? firstShot = null;
        private Shot? lastShot = null;
        private Ship.ShipOrientation? shipOrientation = null;
        private Direction? direction = null;
        private List<Direction> possibleDirections = [];

        protected abstract Shot GetNextShot();

        public abstract bool NextShot();

        protected bool DeterminedNextShot(Func<Shot> nextShotFunction, bool random = false)
        {
            do
            {
                if (random || firstShot == null)
                {
                    Shot nextShot = nextShotFunction();

                    Orientation determinedOrientation = DetermineShipOrientation(nextShot.Row, nextShot.Col);
                    if (determinedOrientation != Orientation.None)
                    {
                        if (determinedOrientation == Orientation.Vertical)
                        {
                            shipOrientation = Ship.ShipOrientation.Vertical;
                        }
                        else if (determinedOrientation == Orientation.Horizontal)
                        {
                            shipOrientation = Ship.ShipOrientation.Horizontal;
                        }
                        else
                        {
                            shipOrientation = null;
                        }

                        Playground.ShotResult shotResult = playground.Shot(nextShot.Row, nextShot.Col);
                        if (shotResult != Playground.ShotResult.None)
                        {
                            if (shotResult == Playground.ShotResult.Miss)
                            {
                                return false;
                            }
                            else
                            {
                                if (shotResult == Playground.ShotResult.Hit)
                                {
                                    firstShot = new Shot(nextShot.Row, nextShot.Col);
                                }
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    direction ??= DetermineRandomDirection();

                    int row = lastShot == null ? firstShot.Row : lastShot.Row;
                    int col = lastShot == null ? firstShot.Col : lastShot.Col;

                    switch (direction)
                    {
                        case Direction.Up:
                            row--;
                            break;
                        case Direction.Down:
                            row++;
                            break;
                        case Direction.Left:
                            col--;
                            break;
                        case Direction.Right:
                            col++;
                            break;
                    }

                    if (row < 0 || row >= gameSetting.FieldSize || col < 0 || col >= gameSetting.FieldSize)
                    {
                        ChangeDirection();
                        continue;
                    }
                    else
                    {
                        Playground.ShotResult shotResult = playground.Shot(row, col);

                        if (shotResult != Playground.ShotResult.None)
                        {
                            if (shotResult == Playground.ShotResult.Miss)
                            {
                                ChangeDirection();
                                return false;
                            }
                            else
                            {
                                if (shotResult == Playground.ShotResult.Hit)
                                {
                                    lastShot = new Shot(row, col);
                                    // Bestimme die Orientierung basierend auf den Schüssen
                                    shipOrientation ??= firstShot.Row == lastShot.Row
                                        ? Ship.ShipOrientation.Horizontal
                                        : Ship.ShipOrientation.Vertical;
                                }
                                else
                                {
                                    Reset();
                                }
                                return true;
                            }
                        }
                        else
                        {
                            ChangeDirection();
                        }
                    }
                }
            }
            while (true);
        }

        // Hilfsmethode zur Richtungsbestimmung
        private Direction DetermineRandomDirection()
        {
            if (shipOrientation == Ship.ShipOrientation.Horizontal)
            {
                possibleDirections = [Direction.Left, Direction.Right];
                Direction directionHorizontal = possibleDirections[new Random().Next(0, 2)];
                possibleDirections.Remove(directionHorizontal);
                return directionHorizontal;
            }
            else if (shipOrientation == Ship.ShipOrientation.Vertical)
            {
                possibleDirections = [Direction.Up, Direction.Down];
                Direction directionVertical = possibleDirections[new Random().Next(0, 2)];
                possibleDirections.Remove(directionVertical);
                return directionVertical;
            }
            possibleDirections = [Direction.Up, Direction.Down, Direction.Left, Direction.Right];
            Direction direction = possibleDirections[new Random().Next(0, 4)];
            possibleDirections.Remove(direction);
            return direction;
        }

        private void ChangeDirection()
        {
            if (shipOrientation != null)
            {
                possibleDirections = [Direction.Left, Direction.Up, Direction.Right, Direction.Down];
                direction = (Direction)(((int)direction! + 2) % 4);
                possibleDirections.Clear();
            }
            else
            {
                int index = (shipOrientation != null && possibleDirections.Count > 1) ? 1 : new Random().Next(0, possibleDirections.Count - 1);
                Direction currentDirection = possibleDirections[index];
                possibleDirections.RemoveAt(index);
                direction = currentDirection;
            }
            lastShot = null;
        }

        private void Reset()
        {
            firstShot = null;
            lastShot = null;
            shipOrientation = null;
            direction = null;
            possibleDirections = [Direction.Left, Direction.Up, Direction.Right, Direction.Down];
        }

        protected enum Orientation
        {
            None,
            Horizontal,
            Vertical,
            Both
        }
        protected Orientation DetermineShipOrientation(int row, int col, int i = 0)
        {
            if (gameSetting.GameDifficult == GameSetting.Difficult.Easy)
            {
                return Orientation.Both;
            }
            if (row < 0 || row >= gameSetting.FieldSize || col < 0 || col >= gameSetting.FieldSize)
            {
                //Debug.WriteLine("Field is out of bounds");
                return Orientation.None;
            }

            // Wenn das Feld Wasser oder Schiff ist, mache weiter
            if (playground.Field[row, col] != (int)Playground.FieldState.Water &&
                playground.Field[row, col] != (int)Playground.FieldState.Ship)
            {
                //Debug.WriteLine("Field is not water or ship");
                return Orientation.None;
            }

            int shipSize = i != 0 ? i : DetermineShipSize();

            int vertical = CheckDirection(row + 1, col, 1, 0) + CheckDirection(row - 1, col, -1, 0) + 1;
            int horizontal = CheckDirection(row, col + 1, 0, 1) + CheckDirection(row, col - 1, 0, -1) + 1;

            /*Debug.WriteLine("Horizontal: " + horizontal);
            Debug.WriteLine("Vertical: " + vertical);*/

            if (horizontal >= shipSize && vertical >= shipSize)
            {
                return Orientation.Both;
            }
            else if (horizontal >= shipSize)
            {
                return Orientation.Horizontal;
            }
            else if (vertical >= shipSize)
            {
                return Orientation.Vertical;
            }
            else
            {
                return Orientation.None;
            }
        }

        private int DetermineShipSize()
        {
            int carrierAmount = playground.Ships.FindAll(ship => ship.shipType == Ship.ShipType.Carrier).Count;
            if (carrierAmount > 0)
            {
                return (int)Ship.ShipType.Carrier;
            }
            int battleshipAmount = playground.Ships.FindAll(ship => ship.shipType == Ship.ShipType.Battleship).Count;
            if (battleshipAmount > 0)
            {
                return (int)Ship.ShipType.Battleship;
            }
            int cruiserAmount = playground.Ships.FindAll(ship => ship.shipType == Ship.ShipType.Cruiser).Count;
            if (cruiserAmount > 0)
            {
                return (int)Ship.ShipType.Cruiser;
            }
            int submarineAmount = playground.Ships.FindAll(ship => ship.shipType == Ship.ShipType.Submarine).Count;
            if (submarineAmount > 0)
            {
                return (int)Ship.ShipType.Submarine;
            }
            int destroyerAmount = playground.Ships.FindAll(ship => ship.shipType == Ship.ShipType.Destroyer).Count;
            if (destroyerAmount > 0)
            {
                return (int)Ship.ShipType.Destroyer;
            }
            return 0; // Falls kein Schiff verfügbar ist
        }

        private int CheckDirection(int row, int col, int rowStep, int colStep)
        {
            int count = 0;

            int i = 0;
            int newRow = row + rowStep * i;
            int newCol = col + colStep * i;

            while (newRow >= 0 && newRow < gameSetting.FieldSize && newCol >= 0 && newCol < gameSetting.FieldSize)
            {
                // Wenn das Feld Wasser oder Schiff ist, zähle es
                if (playground.Field[newRow, newCol] == (int)Playground.FieldState.Water ||
                    playground.Field[newRow, newCol] == (int)Playground.FieldState.Ship)
                {
                    count++;
                }
                else
                {
                    break;
                }

                i++;
                newRow = row + rowStep * i;
                newCol = col + colStep * i;
            }

            return count;
        }
    }

    public class Shot(int row, int col)
    {
        public int Row { get; set; } = row;
        public int Col { get; set; } = col;
    }
}
