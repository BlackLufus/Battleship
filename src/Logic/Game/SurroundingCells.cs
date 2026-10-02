using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Logic.Game
{
    public class SurroundingCells
    {
        /// <summary>
        /// Block or unblock surrounding cells of a ship
        /// </summary>
        /// <param name="board">The board to add blocked positions</param>
        /// <param name="boardSize">The size of the board</param>
        /// <param name="ship">The ship to block surrounding cells for</param>
        /// <param name="add">True to block, false to unblock</param>
        public static void BlockSurroundingCells(
            int[,] board,
            int boardSize,
            Ship ship,
            bool add = true
        )
        {
            int len = (int)ship.type;
            int delta = add ? 1 : -1;

            if (ship.orientation == ShipOrientation.Horizontal)
            {
                for (int i = -1; i <= len; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        int r = ship.row + j;
                        int c = ship.col + i;

                        if (r >= 0 && r < boardSize && c >= 0 && c < boardSize)
                        {
                            board[r, c] += delta;
                        }
                    }
                }
            }
            else
            {
                for (int i = -1; i <= len; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        int r = ship.row + i;
                        int c = ship.col + j;

                        if (r >= 0 && r < boardSize && c >= 0 && c < boardSize)
                        {
                            board[r, c] += delta;
                        }
                    }
                }
            }
        }
    }
}
