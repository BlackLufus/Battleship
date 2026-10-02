using Battleship.Core.Game;
using Battleship.Core.Global;

namespace Battleship.Core.BattelStrategy.Modes
{
    public class Easy(GameSetting gameSetting, PlaygroundBoardLogic boardLogic)
        : GameAILogic(gameSetting, boardLogic)
    {
        private readonly GameSetting gameSetting = gameSetting;
        private readonly PlaygroundBoardLogic boardLogic = boardLogic;

        protected override Shot GetNextShot()
        {
            int row = new Random().Next(0, gameSetting.BoardSize);
            int col = new Random().Next(0, gameSetting.BoardSize);

            return new Shot(row, col);
        }

        public override bool NextShot()
        {
            return DeterminedNextShot(GetNextShot);
        }
    }
}
