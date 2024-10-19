using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleship.Global;

namespace Battleship.Playground.ComputerLogic
{
    public class GameAI
    {
        private readonly GameSetting gameSetting;
        private readonly Playground playground;

        private Easy? easy;
        private Medium? medium;
        private Hard? hard;

        public GameAI(GameSetting gameSetting, Playground playground)
        {
            this.gameSetting = gameSetting;
            this.playground = playground;

            Setup();
        }

        private void Setup()
        {
            switch (gameSetting.GameDifficult)
            {
                case GameSetting.Difficult.VeryEasy:
                    easy = new Easy(gameSetting, playground);
                    break;
                case GameSetting.Difficult.Easy:
                    easy = new Easy(gameSetting, playground);
                    break;
                case GameSetting.Difficult.Medium:
                    medium = new Medium(gameSetting, playground);
                    break;
                case GameSetting.Difficult.Hard:
                    hard = new Hard(gameSetting, playground);
                    break;
                case GameSetting.Difficult.VeryHard:
                    hard = new Hard(gameSetting, playground, true);
                    break;
            }
        }

        public bool NextShot()
        {
            return gameSetting.GameDifficult switch
            {
                GameSetting.Difficult.VeryEasy => easy!.NextShot(),
                GameSetting.Difficult.Easy => easy!.NextShot(),
                GameSetting.Difficult.Medium => medium!.NextShot(),
                GameSetting.Difficult.Hard => hard!.NextShot(),
                GameSetting.Difficult.VeryHard => hard!.NextShot(),
                _ => false
            };
        }
    }
}
