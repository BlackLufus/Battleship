using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleship.Logic.Game;
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
            var board = boardLogic.GetBoardState();

            // Schachbrettmuster-Felder finden, die noch nicht beschossen wurden
            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize; col++)
                {
                    //if ((row + col) % 2 == 0 && (board[row, col] == CellState.WATER || board[row, col] == CellState.SHIP)) // Gittermusterbedingung
                        if ((row + col) % 2 == 0 && !boardLogic.CellWasShot(row, col)) // Gittermusterbedingung
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
                        //if ((board[row, col] == CellState.WATER || board[row, col] == CellState.SHIP))
                        if (!boardLogic.CellWasShot(row, col))
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
