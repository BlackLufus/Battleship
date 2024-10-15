using Battleship.Lobby;
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
            probability = new int[gameSetting.FieldSize, gameSetting.FieldSize];

            for (int row = 0; row < gameSetting.FieldSize; row++)
            {
                for (int col = 0; col < gameSetting.FieldSize; col++)
                {
                    probability[row, col] = 0;
                }
            }
        }

        private void DetermineProbability()
        {
            for (int row = 0; row < gameSetting.FieldSize; row++)
            {
                for (int col = 0; col < gameSetting.FieldSize; col++)
                {
                    if (!playground.WasShot(row, col))
                    {
                        foreach (var ship in playground.Ships)
                        {
                            if (!ship.IsSunk)
                            {
                                Orientation orientation = DetermineShipOrientation(row, col, (int)ship.shipType);

                                if (orientation != Orientation.None)
                                {
                                    if (orientation == Orientation.Horizontal || orientation == Orientation.Both)
                                    {
                                        if (col + (int)ship.shipType - 1 < gameSetting.FieldSize)
                                        {
                                            for (int i = 0; i < (int)ship.shipType; i++)
                                            {
                                                if (!playground.WasShot(row, col + i))
                                                {
                                                    probability![row, col + i]++;
                                                }
                                            }
                                        }
                                        if (col - (int)ship.shipType + 1 >= 0)
                                        {
                                            for (int i = 0; i < (int)ship.shipType; i++)
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
                                        if (row + (int)ship.shipType - 1 < gameSetting.FieldSize)
                                        {
                                            for (int i = 0; i < (int)ship.shipType; i++)
                                            {
                                                if (!playground.WasShot(row + i, col))
                                                {
                                                    probability![row + i, col]++;
                                                }
                                            }
                                        }
                                        if (row - (int)ship.shipType + 1 >= 0)
                                        {
                                            for (int i = 0; i < (int)ship.shipType; i++)
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

            if (ship.shipOrientation == Ship.ShipOrientation.Vertical)
            {
                int row = 0;
                int col = 0;
                do
                {
                    row = new Random().Next(ship.row, ship.row + (int)ship.shipType - 1);
                    col = ship.column;
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
                    col = new Random().Next(ship.column, ship.column + (int)ship.shipType - 1);
                }
                while (playground.WasShot(row, col));
                return new Shot(row, col);
            }
        }
        protected override Shot GetNextShot()
        {
            if (claivoyantAbilities && nextClaivoyantShot == 0)
            {
                nextClaivoyantShot = new Random().Next(2, 3);
                return ClairvoyantShot();
            }
            InitializeProbability();
            DetermineProbability();
            Dump();
            int max = 0;
            List<Shot> maxShots = [];

            for (int row = 0; row < gameSetting.FieldSize; row++)
            {
                for (int col = 0; col < gameSetting.FieldSize; col++)
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
            // Wähle zufällig einen der besten Schüsse
            return maxShots[new Random().Next(0, maxShots.Count)];
        }

        public override bool NextShot()
        {
            return DeterminedNextShot(GetNextShot);
        }

        private void Dump()
        {
            for (int row = 0; row < gameSetting.FieldSize; row++)
            {
                for (int col = 0; col < gameSetting.FieldSize; col++)
                {
                    Debug.Write((probability[row, col].ToString().Length == 1 ? "  " + probability[row, col] : probability[row, col].ToString().Length == 2 ? " " + probability[row, col] : probability[row, col]) + " ");
                }
                Debug.WriteLine("");
            }
        }
    }
}
