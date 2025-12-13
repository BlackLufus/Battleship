using Battleship.Logic.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Battleship.Logic
{
    public class BoardLogic
    {
        public enum ShotResult
        {
            Miss = -1,
            None = 0,
            Hit = 2,
            Sunk = 3
        }

        public enum CellState
        {
            BLOCKED = -2,
            MISS = -1,
            WATER = 0,
            SHIP = 1,
            HIT = 2,
            SUNK = 3
        }

        private readonly int boardSize;
        private readonly List<DragShip> dragShips;

        private readonly List<(int x, int y)> shots = [];
        private readonly int[,] board;

        public BoardLogic(int boardSize, List<DragShip> dragShips)
        {
            this.boardSize = boardSize;
            this.dragShips = dragShips;

            board = new int[boardSize, boardSize];
        }

        public ShotResult Shot(int row, int col)
        {
            if (row < 0 || row >= boardSize ||  
                col < 0 || col >= boardSize ||
                shots.Contains((row, col)))
                return ShotResult.None;
            shots.Add((row, col));
            foreach (DragShip ship in dragShips)
            {
                if (ship.Hit(row, col))
                {
                    if (ship.IsSunk)
                    {
                        BoardCells.BlockSurroundingCells(board, boardSize, ship);
                        return ShotResult.Sunk;
                    }
                    return ShotResult.Hit;
                }
            }
            return ShotResult.Miss;
        }

        public bool IsAllSunk()
        {
            return dragShips.All(ship => ship.IsSunk);
        }

        public CellState[,] GetBoardState()
        {
            CellState[,] boardState = new CellState[boardSize, boardSize];
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (board[i, j] > 0)
                    {
                        boardState[i, j] = CellState.BLOCKED;
                    }
                    else
                    {
                        boardState[i, j] = CellState.WATER;
                    }
                }
            }
            foreach (Ship ship in dragShips)
            {
                if (ship.orientation == Ship.ShipOrientation.Horizontal)
                {
                    for (int i = 0; i < (int)ship.type; i++)
                    {
                        if (ship.IsSunk)
                            boardState[ship.row, ship.col + i] = CellState.SUNK;
                        else if (ship.Shots.Contains((ship.row, ship.col + i)))
                            boardState[ship.row, ship.col + i] = CellState.HIT;
                        else
                            boardState[ship.row, ship.col + i] = CellState.SHIP;
                    }
                }
                else
                {
                    for (int i = 0; i < (int)ship.type; i++)
                    {
                        if (ship.IsSunk)
                            boardState[ship.row + i, ship.col] = CellState.SUNK;
                        else if (ship.Shots.Contains((ship.row + i, ship.col)))
                            boardState[ship.row + i, ship.col] = CellState.HIT;
                        else
                            boardState[ship.row + i, ship.col] = CellState.SHIP;
                    }
                }
            }
            return boardState;
        }
    }
}
