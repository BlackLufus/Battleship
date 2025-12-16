using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battleship.Logic.Board;
using Battleship.Logic.Global;

namespace Battleship.Logic.BattelStrategy.Modes
{
    public class GameAI
    {
        private readonly GameSetting gameSetting;
        private readonly PlaygroundBoardLogic boardLogic;

        private Easy? easy;
        private Medium? medium;
        private Hard? hard;

        public GameAI(GameSetting gameSetting, PlaygroundBoardLogic boardLogic)
        {
            this.gameSetting = gameSetting;
            this.boardLogic = boardLogic;

            Setup();
        }

        private void Setup()
        {
            switch (gameSetting.GameDifficult)
            {
                case GameSetting.Difficult.VeryEasy:
                    easy = new Easy(gameSetting, boardLogic);
                    break;
                case GameSetting.Difficult.Easy:
                    easy = new Easy(gameSetting, boardLogic);
                    break;
                case GameSetting.Difficult.Medium:
                    medium = new Medium(gameSetting, boardLogic);
                    break;
                case GameSetting.Difficult.Hard:
                    hard = new Hard(gameSetting, boardLogic);
                    break;
                case GameSetting.Difficult.VeryHard:
                    hard = new Hard(gameSetting, boardLogic, true);
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
