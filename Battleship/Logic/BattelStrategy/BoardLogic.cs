using Battleship.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Logic.BattelStrategy
{
    public class BoardLogic (int boardSize, List<Ship> ships)
    {
        public enum ShotResult
        {
            Miss = -1,
            None = 0,
            Hit = 2,
            Sunk = 3
        }

        public enum FieldState
        {
            Restrict = -2,
            Miss = -1,
            Water = 0,
            Ship = 1,
            Hit = 2,
            Sunk = 3
        }
    }
}
