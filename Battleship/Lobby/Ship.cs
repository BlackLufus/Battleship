namespace Battleship.Lobby
{
    public class Ship : IDisposable
    {
        public enum ShipType
        {
            Carrier = 1,
            Battleship = 2,
            Cruiser = 3,
            Submarine = 4,
            Destroyer = 5
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

        private int health;
        private List<(int, int)> shots = [];

        public int Health => health;
        public bool IsSunk => health == 0;

        public Ship(ShipType shipType, ShipOrientation shipOrientation)
        {
            this.shipType = shipType;
            this.shipOrientation = shipOrientation;
            health = (int)shipType;
        }

        public Ship(int row, int column, ShipType shipType, ShipOrientation shipOrientation)
        {
            this.row = row;
            this.column = column;
            this.shipType = shipType;
            this.shipOrientation = shipOrientation;
        }

        public bool IsHit(int row, int column)
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
                    health--;
                    return true;
                }
                return false;
            }
            else
            {
                if (this.column == column && row >= this.row && row < this.row + (int)shipType) {
                    shots.Add((row, column));
                    health--;
                    return true;
                }
                return false;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }
}
