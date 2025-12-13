using System.Runtime.Intrinsics.X86;

namespace Battleship.Global
{
    public class Ship : IDisposable
    {
        public enum ShipType
        {
            Carrier = 1,
            Battleship = 2,
            Cruiser = 3,
            Submarine = 4,
            Destroyer = 5,
        }

        public enum ShipOrientation
        {
            Vertical = 0,
            Horizontal = 270
        }

        public int row;
        public int col;
        public ShipType type;
        public ShipOrientation orientation;

        private readonly List<(int, int)> shots = [];

        public int Health => (int)type - shots.Count;
        public List<(int, int)> Shots => shots;
        public bool IsSunk => Health == 0;

        public Ship(ShipType type, ShipOrientation orientation)
        {
            this.type = type;
            this.orientation = orientation;
        }

        public Ship(int row, int col, ShipType type, ShipOrientation orientation)
        {
            this.row = row;
            this.col = col;
            this.type = type;
            this.orientation = orientation;
        }

        public bool Hit(int row, int col)
        {
            if (shots.Contains((row, col)))
            {
                return false;
            }
            else if (HasPosition(row, col))
            {
                shots.Add((row, col));
                return true;
            }
            return false;
        }

        public bool HasPosition(int row, int col)
        {
            if (orientation == ShipOrientation.Horizontal)
            {
                return this.row == row && col >= this.col && col < this.col + (int)type;
            }
            else
            {
                return this.col == col && row >= this.row && row < this.row + (int)type;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
