using Battleship.Core.Game;
using Battleship.Core.Global;

namespace Battleship.Core.BattelStrategy.Modes
{
    public abstract class GameAILogic(GameSetting gameSetting, PlaygroundBoardLogic boardLogic)
    {
        protected enum Orientation
        {
            None,
            Horizontal,
            Vertical,
            Both,
        }

        enum Direction
        {
            Right = 0,
            Up = 1,
            Left = 2,
            Down = 3,
        }

        private readonly GameSetting gameSetting = gameSetting;
        private readonly PlaygroundBoardLogic boardLogic = boardLogic;

        private Shot? firstShot = null;
        private Shot? lastShot = null;
        private ShipOrientation? shipOrientation = null;
        private Direction? direction = null;
        private List<Direction> possibleDirections = [];

        protected abstract Shot GetNextShot();

        public abstract bool NextShot();

        /// <summary>
        /// Determine the next shoot
        /// </summary>
        /// <param name="nextShotFunction"></param>
        /// <returns></returns>
        protected bool DeterminedNextShot(Func<Shot> nextShotFunction)
        {
            do
            {
                if (firstShot == null)
                {
                    Shot nextShot = nextShotFunction();

                    Orientation determinedOrientation = DetermineShipOrientation(
                        nextShot.Row,
                        nextShot.Col
                    );
                    if (
                        gameSetting.GameDifficult == GameSetting.Difficult.VeryEasy
                        || determinedOrientation != Orientation.None
                    )
                    {
                        if (determinedOrientation == Orientation.Vertical)
                        {
                            shipOrientation = ShipOrientation.Vertical;
                        }
                        else if (determinedOrientation == Orientation.Horizontal)
                        {
                            shipOrientation = ShipOrientation.Horizontal;
                        }
                        else
                        {
                            shipOrientation = null;
                        }

                        ShotResult shotResult = boardLogic.Shoot(nextShot.Row, nextShot.Col);
                        if (shotResult != ShotResult.None)
                        {
                            if (shotResult == ShotResult.Miss)
                            {
                                return false;
                            }
                            else
                            {
                                if (
                                    shotResult == ShotResult.Hit
                                    && gameSetting.GameDifficult != GameSetting.Difficult.VeryEasy
                                )
                                {
                                    firstShot = new Shot(nextShot.Row, nextShot.Col);
                                }
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    direction ??= DetermineRandomDirection();

                    int row = lastShot == null ? firstShot.Row : lastShot.Row;
                    int col = lastShot == null ? firstShot.Col : lastShot.Col;

                    switch (direction)
                    {
                        case Direction.Up:
                            row--;
                            break;
                        case Direction.Down:
                            row++;
                            break;
                        case Direction.Left:
                            col--;
                            break;
                        case Direction.Right:
                            col++;
                            break;
                    }

                    if (
                        row < 0
                        || row >= gameSetting.BoardSize
                        || col < 0
                        || col >= gameSetting.BoardSize
                    )
                    {
                        ChangeDirection();
                        continue;
                    }
                    else
                    {
                        ShotResult shotResult = boardLogic.Shoot(row, col);

                        if (shotResult != ShotResult.None)
                        {
                            if (shotResult == ShotResult.Miss)
                            {
                                ChangeDirection();
                                return false;
                            }
                            else
                            {
                                if (shotResult == ShotResult.Hit)
                                {
                                    lastShot = new Shot(row, col);
                                    // Bestimme die Orientierung basierend auf den Schüssen
                                    shipOrientation ??=
                                        firstShot.Row == lastShot.Row
                                            ? ShipOrientation.Horizontal
                                            : ShipOrientation.Vertical;
                                }
                                else
                                {
                                    ResetVariables();
                                }
                                return true;
                            }
                        }
                        else
                        {
                            ChangeDirection();
                        }
                    }
                }
            } while (true);
        }

        /// <summary>
        /// Auxiliary method for determining direction
        /// </summary>
        /// <returns>The selected direction</returns>
        private Direction DetermineRandomDirection()
        {
            if (shipOrientation == ShipOrientation.Horizontal)
            {
                possibleDirections = [Direction.Left, Direction.Right];
                Direction directionHorizontal = possibleDirections[new Random().Next(0, 2)];
                possibleDirections.Remove(directionHorizontal);
                return directionHorizontal;
            }
            else if (shipOrientation == ShipOrientation.Vertical)
            {
                possibleDirections = [Direction.Up, Direction.Down];
                Direction directionVertical = possibleDirections[new Random().Next(0, 2)];
                possibleDirections.Remove(directionVertical);
                return directionVertical;
            }
            else
            {
                possibleDirections =
                [
                    Direction.Up,
                    Direction.Down,
                    Direction.Left,
                    Direction.Right,
                ];
                Direction direction = possibleDirections[new Random().Next(0, 4)];
                possibleDirections.Remove(direction);
                return direction;
            }
        }

        /// <summary>
        /// Change the current direction
        /// </summary>
        private void ChangeDirection()
        {
            if (shipOrientation != null)
            {
                possibleDirections =
                [
                    Direction.Left,
                    Direction.Up,
                    Direction.Right,
                    Direction.Down,
                ];
                direction = (Direction)(((int)direction! + 2) % 4);
                possibleDirections.Clear();
            }
            else
            {
                int index =
                    shipOrientation != null && possibleDirections.Count > 1
                        ? 1
                        : new Random().Next(0, possibleDirections.Count - 1);
                Direction currentDirection = possibleDirections[index];
                possibleDirections.RemoveAt(index);
                direction = currentDirection;
            }
            lastShot = null;
        }

        /// <summary>
        /// Reset all variables
        /// </summary>
        private void ResetVariables()
        {
            firstShot = null;
            lastShot = null;
            shipOrientation = null;
            direction = null;
            possibleDirections = [Direction.Left, Direction.Up, Direction.Right, Direction.Down];
        }

        /// <summary>
        /// Determine ship orientations matching the ship size
        /// </summary>
        /// <param name="row">The position in row</param>
        /// <param name="col">The position in col</param>
        /// <param name="shipSize">The size of the ship to check</param>
        /// <returns>Returns the dimension corresponding to the ship size.</returns>
        protected Orientation DetermineShipOrientation(int row, int col, int shipSize = 0)
        {
            // Ignore if difficult is easy
            if (gameSetting.GameDifficult == GameSetting.Difficult.Easy)
                return Orientation.Both;

            // Row or col is outside the board size
            if (row < 0 || row >= gameSetting.BoardSize || col < 0 || col >= gameSetting.BoardSize)
                return Orientation.None;

            // Cell is was not shot yet
            if (boardLogic.CellWasShot(row, col))
                return Orientation.None;

            // Determine the smallest still available ship
            shipSize = shipSize > 0 ? shipSize : boardLogic.DeterminedSmallestShipSize();

            // Determine the vertical and horizontal dimensions
            int vertical =
                CalculateDirectionalDimension(row + 1, col, 1, 0)
                + CalculateDirectionalDimension(row - 1, col, -1, 0)
                + 1;
            int horizontal =
                CalculateDirectionalDimension(row, col + 1, 0, 1)
                + CalculateDirectionalDimension(row, col - 1, 0, -1)
                + 1;

            /*Debug.WriteLine("Horizontal: " + horizontal);
            Debug.WriteLine("Vertical: " + vertical);
            Debug.WriteLine("ShipSize: " + shipSize);*/

            // Return Both if both the horizontal and vertical dimensions match the ship size.
            if (horizontal >= shipSize && vertical >= shipSize)
                return Orientation.Both;
            // Return Horizontal if horizontal dimensions match the ship size
            else if (horizontal >= shipSize)
                return Orientation.Horizontal;
            // Return Vertical vertical dimensions match the ship size
            else if (vertical >= shipSize)
                return Orientation.Vertical;
            // Return None if none dimensions match the ship size
            else
                return Orientation.None;
        }

        /// <summary>
        /// Calculate the directional dimension
        /// </summary>
        /// <param name="row">Start position in row</param>
        /// <param name="col">Start position in col</param>
        /// <param name="rowStep">Step size in row direction</param>
        /// <param name="colStep">Step size in col direction</param>
        /// <returns>The dimension in the specific direction</returns>
        private int CalculateDirectionalDimension(int row, int col, int rowStep, int colStep)
        {
            // Counter with steps until the cell at position tempRow|tempCol has been shot or cell is out of boundaries
            int count = 0;

            // Variables in current cell
            int i = 0;
            int tempRow = row + rowStep * i;
            int tempCol = col + colStep * i;

            while (
                tempRow >= 0
                && tempRow < gameSetting.BoardSize
                && tempCol >= 0
                && tempCol < gameSetting.BoardSize
            )
            {
                // Increment count if the cell has not yet been shot at.
                if (!boardLogic.CellWasShot(tempRow, tempCol))
                    count++;
                // Otherwise break the loop
                else
                    break;

                // Increment index
                i++;
                tempRow = row + rowStep * i;
                tempCol = col + colStep * i;
            }

            // Return counter
            return count;
        }
    }

    public class Shot(int row, int col)
    {
        public int Row { get; set; } = row;
        public int Col { get; set; } = col;
    }
}
