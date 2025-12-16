using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Logic.Global
{
    public class Variables
    {
        private Variables() { }

        private static string username = "Spieler" + new Random().Next(1000, 9999);
        public static string Username
        {
            get => username;
            set => username = value;
        }
    }
}
