using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleship.Logic.Board;
using Battleship.Logic.Global;

namespace Battleship.Logic.BattelStrategy.Modes
{
    public class Medium(GameSetting gameSetting, PlaygroundBoardLogic boardLogic) : GameAILogic(gameSetting, boardLogic)
    {
        private readonly GameSetting gameSetting = gameSetting;
        private readonly PlaygroundBoardLogic boardLogic = boardLogic;

        protected override Shot GetNextShot()
        {
            List<Shot> possibleShots = [];

            // Schachbrettmuster-Felder finden, die noch nicht beschossen wurden
            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize; col++)
                {
                    if ((row + col) % 2 == 0 && !boardLogic.WasShot(row, col)) // Gittermusterbedingung
                    {
                        possibleShots.Add(new Shot(row, col)); // Füge das Feld zu möglichen Schüssen hinzu
                    }
                }
            }

            if (possibleShots.Count == 0)
            {
                for (int row = 0; row < gameSetting.BoardSize; row++)
                {
                    for (int col = 0; col < gameSetting.BoardSize; col++)
                    {
                        if (!boardLogic.WasShot(row, col))
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
