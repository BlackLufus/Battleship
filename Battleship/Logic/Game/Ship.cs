using System.Runtime.Intrinsics.X86;
using System.Windows.Media;

namespace Battleship.Logic.Game
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

    public class Ship : IDisposable
    {
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

        /// <summary>
        /// Hit the Ship at position row, col
        /// </summary>
        /// <param name="row">Position at row</param>
        /// <param name="col">Position at col</param>
        /// <returns>Returns true when ship was hit otherwise false</returns>
        public bool Hit(int row, int col)
        {
            // Return false when row and col does not hit the ship
            if (!HasPosition(row, col))
                return false;

            // Return false when new shoot is already in shots
            if (shots.Contains((row, col)))
                return false;

            // Add to shots to prevent multiple shots at the same position
            shots.Add((row, col));
            return true;
        }

        /// <summary>
        /// Is Position at ship
        /// </summary>
        /// <param name="row">Position at row</param>
        /// <param name="col">Position at col</param>
        /// <returns>Return true when position is at ship otherwise false</returns>
        public bool HasPosition(int row, int col)
        {
            if (orientation == ShipOrientation.Horizontal)
                return this.row == row && col >= this.col && col < this.col + (int)type;
            else
                return this.col == col && row >= this.row && row < this.row + (int)type;
        }

        /// <summary>
        /// Dispose a ship object
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
