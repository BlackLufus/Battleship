namespace Battleship.Core.Global
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
