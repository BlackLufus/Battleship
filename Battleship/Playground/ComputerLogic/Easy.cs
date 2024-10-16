using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleship.Global;

namespace Battleship.Playground.ComputerLogic
{
    public class Easy(GameSetting gameSetting, Playground playground, bool randomly = false) : GameAILogic(gameSetting, playground)
    {
        private readonly GameSetting gameSetting = gameSetting;
        private readonly Playground playground = playground;
        private readonly bool randomly = randomly;

        override protected Shot GetNextShot()
        {
            int row = new Random().Next(0, gameSetting.FieldSize);
            int col = new Random().Next(0, gameSetting.FieldSize);

            return new Shot(row, col);
        }

        override public bool NextShot()
        {
            return DeterminedNextShot(GetNextShot, randomly);
        }
    }
}
