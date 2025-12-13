using Battleship.Global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Battleship.Playground.ComputerLogic
{

    public class Hard : GameAILogic
    {
        private readonly GameSetting gameSetting;
        private readonly Playground playground;
        private int[,]? probability;
        private int nextClaivoyantShot = 0;
        private readonly bool claivoyantAbilities;

        public Hard(GameSetting gameSetting, Playground playground, bool claivoyantAbilities = false) : base(gameSetting, playground)
        {
            this.gameSetting = gameSetting;
            this.playground = playground;
            this.claivoyantAbilities = claivoyantAbilities;
        }

        private void InitializeProbability()
        {
            probability = new int[gameSetting.BoardSize, gameSetting.BoardSize];

            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize; col++)
                {
                    probability[row, col] = 0;
                }
            }
        }

        private void DetermineProbability()
        {
            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize; col++)
                {
                    if (!playground.WasShot(row, col))
                    {
                        foreach (var ship in playground.Ships)
                        {
                            if (!ship.IsSunk)
                            {
                                Orientation orientation = DetermineShipOrientation(row, col, (int)ship.type);

                                if (orientation != Orientation.None)
                                {
                                    if (orientation == Orientation.Both)
                                    {
                                        probability![row, col]--;
                                    }
                                    if (orientation == Orientation.Horizontal || orientation == Orientation.Both)
                                    {
                                        if (col + (int)ship.type - 1 < gameSetting.BoardSize)
                                        {
                                            for (int i = 0; i < (int)ship.type; i++)
                                            {
                                                if (!playground.WasShot(row, col + i))
                                                {
                                                    probability![row, col + i]++;
                                                }
                                            }
                                        }
                                        if (col - (int)ship.type + 1 >= 0)
                                        {
                                            for (int i = 0; i < (int)ship.type; i++)
                                            {
                                                if (!playground.WasShot(row, col - i))
                                                {
                                                    probability![row, col - i]++;
                                                }
                                            }
                                        }
                                    }
                                    if (orientation == Orientation.Vertical || orientation == Orientation.Both)
                                    {
                                        if (row + (int)ship.type - 1 < gameSetting.BoardSize)
                                        {
                                            for (int i = 0; i < (int)ship.type; i++)
                                            {
                                                if (!playground.WasShot(row + i, col))
                                                {
                                                    probability![row + i, col]++;
                                                }
                                            }
                                        }
                                        if (row - (int)ship.type + 1 >= 0)
                                        {
                                            for (int i = 0; i < (int)ship.type; i++)
                                            {
                                                if (!playground.WasShot(row - i, col))
                                                {
                                                    probability![row - i, col]++;
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
        }

        private Shot ClairvoyantShot()
        {
            int index = new Random().Next(0, playground.Ships.FindAll(ship => !ship.IsSunk).Count);
            Ship ship = playground.Ships.FindAll(ship => !ship.IsSunk)[index];

            if (ship.orientation == Ship.ShipOrientation.Vertical)
            {
                int row = 0;
                int col = 0;
                do
                {
                    row = new Random().Next(ship.row, ship.row + (int)ship.type - 1);
                    col = ship.col;
                }
                while (playground.WasShot(row, col));
                return new Shot(row, col);
            }
            else
            {
                int row = 0;
                int col = 0;
                do
                {
                    row = ship.row;
                    col = new Random().Next(ship.col, ship.col + (int)ship.type - 1);
                }
                while (playground.WasShot(row, col));
                return new Shot(row, col);
            }
        }
        protected override Shot GetNextShot()
        {
            Debug.WriteLine("Hard: GetNextShot (claivoantAbilities: " + claivoyantAbilities + " nextClaivoyantShot: " + nextClaivoyantShot + ")");
            if (claivoyantAbilities && nextClaivoyantShot == 0)
            {
                nextClaivoyantShot = new Random().Next(3, 5);
                return ClairvoyantShot();
            }
            else if (claivoyantAbilities) nextClaivoyantShot--;
            InitializeProbability();
            DetermineProbability();
            Dump();
            int max = 0;
            List<Shot> maxShots = [];

            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize; col++)
                {
                    if (probability![row, col] > max)
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
            maxShots.ForEach(shot => Debug.WriteLine("Hard: GetNextShot: Möglicher Schuss: " + shot.Row + " " + shot.Col));
            // Wähle zufällig einen der besten Schüsse
            return maxShots[new Random().Next(0, maxShots.Count)];
        }

        public override bool NextShot()
        {
            return DeterminedNextShot(GetNextShot);
        }

        private void Dump()
        {
            Debug.WriteLine("=============================================");
            for (int i = 0; i < gameSetting.BoardSize + 1; i++)
            {
                Debug.Write((i.ToString().Length == 1 ? "  " + i : i.ToString().Length == 2 ? " " + i : i) + " ");
            }
            Debug.WriteLine("");
            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize + 1; col++)
                {
                    if (col == 0)
                    {
                        Debug.Write(" " + ((row + 1).ToString().Length == 1 ? " " + (row + 1) : (row + 1)) + " ");
                    }
                    else
                    {
                        Debug.Write((probability![row, col - 1].ToString().Length == 1 ? "  " + probability[row, col - 1] : probability[row, col - 1].ToString().Length == 2 ? " " + probability[row, col - 1] : probability[row, col - 1]) + " ");
                    }
                }
                Debug.WriteLine("");
            }
            Debug.WriteLine("=============================================");
        }
    }
}
