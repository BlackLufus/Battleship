using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleship.Logic.Board;
using Battleship.Logic.Global;

namespace Battleship.Logic.BattelStrategy.Modes
{
    public class Easy(GameSetting gameSetting, PlaygroundBoardLogic boardLogic) : GameAILogic(gameSetting, boardLogic)
    {
        private readonly GameSetting gameSetting = gameSetting;
        private readonly PlaygroundBoardLogic boardLogic = boardLogic;

        override protected Shot GetNextShot()
        {
            int row = new Random().Next(0, gameSetting.BoardSize);
            int col = new Random().Next(0, gameSetting.BoardSize);

            return new Shot(row, col);
        }

        override public bool NextShot()
        {
            return DeterminedNextShot(GetNextShot);
        }
    }
}
