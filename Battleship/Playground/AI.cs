using Battleship.Global;
using Battleship.Lobby;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Battleship.Playground
{
    public class AI(GameSetting gameSetting, BattleshipPlayground playground)
    {
        enum Direction
        {
            Right = 0,
            Up = 1,
            Left = 2,
            Down = 3,
        }

        private readonly GameSetting gameSetting = gameSetting;
        private readonly BattleshipPlayground playground = playground;

        private int[,] probability;

        private Shot? firstShot = null;
        private Shot? lastShot = null;
        private Ship.ShipOrientation? shipOrientation = null;
        private Direction? direction = null;

        private int carrierAmount = gameSetting.CarrierAmount;
        private int battleshipAmount = gameSetting.BattleshipAmount;
        private int cruiserAmount = gameSetting.CruiserAmount;
        private int submarineAmount = gameSetting.SubmarineAmount;
        private int destroyerAmount = gameSetting.DestroyerAmount;

        private void InitializeProbability()
        {
            probability = new int[gameSetting.FieldSize, gameSetting.FieldSize];

            for (int row = 0; row < gameSetting.FieldSize; row++)
            {
                for (int col = 0; col < gameSetting.FieldSize; col++)
                {
                    probability[row, col] = 0;
                }
            }
        }

        private void CalculateProbability()
        {
            InitializeProbability(); // Setzt die Wahrscheinlichkeiten zurück
            for (int row = 0; row < gameSetting.FieldSize; row++)
            {
                for (int col = 0; col < gameSetting.FieldSize; col++)
                {
                    if (!playground.WasShot(row, col)) // Überprüfen, ob das Feld noch nicht beschossen wurde
                    {
                        for (int i = 1; i < 6; i++) // Überprüfe die Schiffe der Längen 1 bis 5
                        {
                            int shipAmount = GetShipAmount(i);
                            for (int iteration = 0; iteration < shipAmount; iteration++)
                            {
                                Orientation orientation = DetermineShipCanBeHere(row, col, i);

                                // Horizontale Platzierung
                                if (orientation == Orientation.Horizontal || orientation == Orientation.Both)
                                {
                                    if (col + i <= gameSetting.FieldSize - 1) // Überprüfe, ob das Schiff passt
                                    {
                                        for (int j = 0; j < i; j++)
                                        {
                                            // Achte darauf, dass wir nicht außerhalb des Arrays zugreifen
                                            if (col + j < gameSetting.FieldSize)
                                            {
                                                if (!playground.WasShot(row, col + j)) // Überprüfen, ob das Feld noch nicht beschossen wurde
                                                    probability[row, col + j]++;
                                            }
                                        }
                                    }
                                }

                                // Vertikale Platzierung
                                if (orientation == Orientation.Vertical || orientation == Orientation.Both)
                                {
                                    if (row + i <= gameSetting.FieldSize - 1) // Überprüfe, ob das Schiff passt
                                    {
                                        for (int j = 0; j < i; j++)
                                        {
                                            // Achte darauf, dass wir nicht außerhalb des Arrays zugreifen
                                            if (row + j < gameSetting.FieldSize)
                                            {
                                                if (!playground.WasShot(row + j, col)) // Überprüfen, ob das Feld noch nicht beschossen wurde
                                                    probability[row + j, col]++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private int GetShipAmount(int shipType)
        {
            switch (shipType)
            {
                case 1: return carrierAmount;
                case 2: return battleshipAmount;
                case 3: return cruiserAmount;
                case 4: return submarineAmount;
                case 5: return destroyerAmount;
                default: return 0; // Sicherheitsmaßnahme
            }
        }

        private Shot NextPrioShot()
        {
            int max = 0;
            List<Shot> maxShots = new List<Shot>();

            for (int row = 0; row < gameSetting.FieldSize; row++)
            {
                for (int col = 0; col < gameSetting.FieldSize; col++)
                {
                    if (probability[row, col] > max)
                    {
                        max = probability[row, col];
                        maxShots.Clear();
                        maxShots.Add(new Shot(row, col));
                    }
                    else if (probability[row, col] == max)
                    {
                        maxShots.Add(new Shot(row, col));
                    }
                }
            }

            // Wenn maxShots leer ist, gibt es keine Schüsse
            if (maxShots.Count == 0)
            {
                throw new InvalidOperationException("Es gibt keine möglichen Schüsse.");
            }

            // Wähle zufällig einen der besten Schüsse
            return maxShots[new Random().Next(0, maxShots.Count)];
        }


        private void Dump()
        {
            for (int row = 0; row < gameSetting.FieldSize; row++)
            {
                for (int col = 0; col < gameSetting.FieldSize; col++)
                {
                    Debug.Write(probability[row, col] + " ");
                }
                Debug.WriteLine("");
            }
        }

        public bool NextShot()
        {
            CalculateProbability();
            Dump();
            Debug.WriteLine("Orientation: " + shipOrientation);
            Debug.WriteLine("Direction: " + direction);
            do
            {
                if (firstShot == null)
                {
                    Shot nextShot = gameSetting.GameDifficult == GameSetting.Difficult.Hard ? NextPrioShot() : NextPosition();

                    Orientation determinedOrientation = DetermineShipCanBeHere(nextShot.Row, nextShot.Col);
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

                        BattleshipPlayground.ShotResult shotResult = playground.Shot(nextShot.Row, nextShot.Col);
                        if (shotResult != BattleshipPlayground.ShotResult.None)
                        {
                            if (shotResult == BattleshipPlayground.ShotResult.Miss)
                            {
                                return false;
                            }
                            else
                            {
                                if (shotResult == BattleshipPlayground.ShotResult.Hit)
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
                    if (direction == null)
                    {
                        direction = DetermineRandomDirection();
                    }

                    Debug.WriteLine("Direction: " + direction);

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
                        direction = ChangeDirection();  // Ändere die Richtung
                        lastShot = null;
                        continue;
                    }
                    else
                    {
                        BattleshipPlayground.ShotResult shotResult = playground.Shot(row, col);

                        if (shotResult != BattleshipPlayground.ShotResult.None)
                        {
                            if (shotResult == BattleshipPlayground.ShotResult.Miss)
                            {
                                direction = ChangeDirection();  // Richtung wechseln nach einem Miss
                                lastShot = null;
                                return false;
                            }
                            else
                            {
                                if (shotResult == BattleshipPlayground.ShotResult.Hit)
                                {
                                    lastShot = new Shot(row, col);
                                    // Bestimme die Orientierung basierend auf den Schüssen
                                    shipOrientation ??= firstShot.Row == lastShot.Row
                                        ? Ship.ShipOrientation.Horizontal
                                        : Ship.ShipOrientation.Vertical;
                                }
                                else
                                {
                                    firstShot = null;
                                    lastShot = null;
                                    shipOrientation = null;
                                    direction = null;
                                }
                                return true;
                            }
                        }
                        else
                        {
                            direction = ChangeDirection();
                            lastShot = null;
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
                return new Random().Next(0, 2) == 0 ? Direction.Left : Direction.Right;
            }
            else if (shipOrientation == Ship.ShipOrientation.Vertical)
            {
                return new Random().Next(0, 2) == 0 ? Direction.Up : Direction.Down;
            }
            return (Direction)new Random().Next(0, 4);  // Zufällige Richtung
        }

        private Direction ChangeDirection()
        {
            return (Direction)(((int)direction! + (shipOrientation == null ? 1 : 2)) % 4);
        }

        private Shot NextPosition(bool smartShot = false)
        {
            List<Shot> possibleShots = new List<Shot>();

            // Schachbrettmuster-Felder finden, die noch nicht beschossen wurden
            for (int row = 0; row < gameSetting.FieldSize; row++)
            {
                for (int col = 0; col < gameSetting.FieldSize; col++)
                {
                    if ((row + col) % 2 == 0 && !playground.WasShot(row, col)) // Gittermusterbedingung
                    {
                        possibleShots.Add(new Shot(row, col)); // Füge das Feld zu möglichen Schüssen hinzu
                    }
                }
            }

            if(possibleShots.Count == 0)
    {
                for (int row = 0; row < gameSetting.FieldSize; row++)
                {
                    for (int col = 0; col < gameSetting.FieldSize; col++)
                    {
                        if (!playground.WasShot(row, col))
                        {
                            possibleShots.Add(new Shot(row, col)); // Füge jedes nicht beschossene Feld hinzu
                        }
                    }
                }
            }

            return possibleShots[new Random().Next(0, possibleShots.Count)];
        }

        private enum Orientation
        {
            None,
            Horizontal,
            Vertical,
            Both
        }
        private Orientation DetermineShipCanBeHere(int row, int col, int i = -1)
        {
            if (gameSetting.GameDifficult == GameSetting.Difficult.Easy)
            {
                return Orientation.Both;
            }
            if (row < 0 || row >= gameSetting.FieldSize || col < 0 || col >= gameSetting.FieldSize)
            {
                Debug.WriteLine("Field is out of bounds");
                return Orientation.None;
            }

            // Wenn das Feld Wasser oder Schiff ist, mache weiter
            if (playground.Field[row, col] != (int)BattleshipPlayground.FieldState.Water &&
                playground.Field[row, col] != (int)BattleshipPlayground.FieldState.Ship)
            {
                Debug.WriteLine("Field is not water or ship");
                return Orientation.None;
            }

            int shipSize = i != -1 ? i : DetermineShipSize();

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
            if (carrierAmount > 0) return 1;
            if (battleshipAmount > 0) return 2;
            if (cruiserAmount > 0) return 3;
            if (submarineAmount > 0) return 4;
            if (destroyerAmount > 0) return 5;
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
                if (playground.Field[newRow, newCol] == (int)BattleshipPlayground.FieldState.Water ||
                    playground.Field[newRow, newCol] == (int)BattleshipPlayground.FieldState.Ship)
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

    public class Shot
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public Shot(int row, int col)
        {
            Row = row;
            Col = col;
        }
    }
}
