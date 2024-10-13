using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Battleship.Global.GameSetting;

namespace Battleship.Global
{
    public class GameSetting(
        Mode gameMode,
        int fieldSize,
        Difficult gameDifficult,
        bool hitBonus,
        bool restricedArea,
        int battleshipAmount,
        int cruiserAmount,
        int submarineAmount,
        int destroyerAmount,
        int carrierAmount = 0)
    {
        public enum Mode
        {
            ComputerVsComputer,
            PlayerVsComputer,
            PlayerVsPlayer
        }

        private readonly Mode gameMode = gameMode;
        public Mode GameMode { get { return gameMode; } }


        private readonly int fieldSize = fieldSize;
        public int FieldSize { get { return fieldSize; } }


        private readonly double singleFieldSize = 300 / (double)fieldSize;
        public double SingleFieldSize { get { return singleFieldSize; } }


        public enum Difficult
        {
            Easy,
            Medium,
            Hard
        }

        private readonly Difficult gameDifficult = gameDifficult;
        public Difficult GameDifficult { get { return gameDifficult; } }


        private readonly bool hitBonus = hitBonus;
        public bool HitBonus { get { return hitBonus; } }


        private readonly bool restricedArea = restricedArea;
        public bool RestricedArea { get { return restricedArea; } }

        private readonly int carrierAmount = carrierAmount;
        public int CarrierAmount { get { return carrierAmount; } }


        private readonly int battleshipAmount = battleshipAmount;
        public int BattleshipAmount { get { return battleshipAmount; } }


        private readonly int cruiserAmount = cruiserAmount;
        public int CruiserAmount { get { return cruiserAmount; } }


        private readonly int submarineAmount = submarineAmount;
        public int SubmarineAmount { get { return submarineAmount; } }


        private readonly int destroyerAmount = destroyerAmount;
        public int DestroyerAmount { get { return destroyerAmount; } }


    }
}
