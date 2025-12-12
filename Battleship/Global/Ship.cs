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
        public int column;
        public ShipType shipType;
        public ShipOrientation shipOrientation;

        private readonly List<(int, int)> shots = [];

        public int Health => (int)shipType - shots.Count;
        public List<(int, int)> Shots => shots;
        public bool IsSunk => Health == 0;

        public Ship(ShipType shipType, ShipOrientation shipOrientation)
        {
            this.shipType = shipType;
            this.shipOrientation = shipOrientation;
        }

        public Ship(int row, int column, ShipType shipType, ShipOrientation shipOrientation)
        {
            this.row = row;
            this.column = column;
            this.shipType = shipType;
            this.shipOrientation = shipOrientation;
        }

        public bool Hit(int row, int column)
        {
            if (shots.Contains((row, column)))
            {
                return false;
            }
            else if (shipOrientation == ShipOrientation.Horizontal)
            {
                if (this.row == row && column >= this.column && column < this.column + (int)shipType)
                {
                    shots.Add((row, column));
                    return true;
                }
                return false;
            }
            else
            {
                if (this.column == column && row >= this.row && row < this.row + (int)shipType)
                {
                    shots.Add((row, column));
                    return true;
                }
                return false;
            }
        }

        public bool HasPosition(int row, int column)
        {
            if (shipOrientation == ShipOrientation.Horizontal)
            {
                return this.row == row && column >= this.column && column < this.column + (int)shipType;
            }
            else
            {
                return this.column == column && row >= this.row && row < this.row + (int)shipType;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
