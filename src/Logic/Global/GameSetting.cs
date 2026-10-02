using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Battleship.Logic.Global.GameSetting;

namespace Battleship.Logic.Global
{
    public class GameSetting(
        Mode gameMode,
        int boardSize,
        Difficult? gameDifficult,
        bool hitBonus,
        bool restrictedArea,
        int battleshipAmount,
        int cruiserAmount,
        int submarineAmount,
        int destroyerAmount,
        int carrierAmount = 0
    )
    {
        public enum Mode
        {
            ComputerVsComputer,
            PlayerVsComputer,
            PlayerVsPlayer,
        }

        private readonly Mode gameMode = gameMode;
        public Mode GameMode
        {
            get { return gameMode; }
        }

        private readonly int boardSize = boardSize;
        public int BoardSize
        {
            get { return boardSize; }
        }

        private readonly double cellSize = 300 / (double)boardSize;
        public double CellSize
        {
            get { return cellSize; }
        }

        public enum Difficult
        {
            VeryEasy,
            Easy,
            Medium,
            Hard,
            VeryHard,
        }

        private readonly Difficult? gameDifficult = gameDifficult;
        public Difficult? GameDifficult
        {
            get { return gameDifficult; }
        }

        private readonly bool hitBonus = hitBonus;
        public bool HitBonus
        {
            get { return hitBonus; }
        }

        private readonly bool restrictedArea = restrictedArea;
        public bool RestrictedArea
        {
            get { return restrictedArea; }
        }

        private readonly int carrierAmount = carrierAmount;
        public int CarrierAmount
        {
            get { return carrierAmount; }
        }

        private readonly int battleshipAmount = battleshipAmount;
        public int BattleshipAmount
        {
            get { return battleshipAmount; }
        }

        private readonly int cruiserAmount = cruiserAmount;
        public int CruiserAmount
        {
            get { return cruiserAmount; }
        }

        private readonly int submarineAmount = submarineAmount;
        public int SubmarineAmount
        {
            get { return submarineAmount; }
        }

        private readonly int destroyerAmount = destroyerAmount;
        public int DestroyerAmount
        {
            get { return destroyerAmount; }
        }
    }
}
