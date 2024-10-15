using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Playground.ComputerLogic
{
    public class Medium(GameSetting gameSetting, Playground playground) : GameAILogic(gameSetting, playground)
    {
        private readonly GameSetting gameSetting = gameSetting;
        private readonly Playground playground = playground;

        protected override Shot GetNextShot()
        {
            List<Shot> possibleShots = [];

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

            if (possibleShots.Count == 0)
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

        public override bool NextShot()
        {
            return DeterminedNextShot(GetNextShot);
        }
    }
}
