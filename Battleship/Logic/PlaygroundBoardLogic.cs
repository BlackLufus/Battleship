using Battleship.Logic.BattelStrategy.Modes;
using Battleship.Logic.Global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Battleship.Logic
{
    public enum CellState
    {
        BLOCKED = -2,
        MISS = -1,
        WATER = 0,
        SHIP = 1,
        HIT = 2,
        SUNK = 3
    }

    public enum ShotResult
    {
        Miss = -1,
        None = 0,
        Hit = 2,
        Sunk = 3
    }

    public class PlaygroundBoardLogic
    {
        private readonly int boardSize;
        private readonly List<DragShip> dragShips;
        public List<Ship> Ships => dragShips.Cast<Ship>().ToList();

        private readonly List<(int x, int y)> shots = [];
        private readonly int[,] board;

        public PlaygroundBoardLogic(int boardSize, List<DragShip> dragShips)
        {
            this.boardSize = boardSize;
            this.dragShips = dragShips;

            board = new int[boardSize, boardSize];
        }

        public void Show(Canvas? canvas = null, bool show = false)
        {
            foreach (DragShip ship in dragShips)
            {
                if (canvas != null)
                    ship.updateCanvas(canvas);
                if (show)
                    ship.Show();
            }
        }

        public ShotResult Shoot(int row, int col, bool show = false)
        {
            if (row < 0 || row >= boardSize ||
                col < 0 || col >= boardSize)
                return ShotResult.None;
            if (board[row, col] > 0 ||
                shots.Contains((row, col)))
                return ShotResult.None;
            shots.Add((row, col));
            foreach (DragShip ship in dragShips)
            {
                if (ship.Shots.Contains((row, col)))
                    return ShotResult.None;
                if (ship.Hit(row, col))
                {
                    if (ship.IsSunk)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (show)
                                ship.Show();
                        });
                        BoardCells.BlockSurroundingCells(board, boardSize, ship);
                        return ShotResult.Sunk;
                    }
                    return ShotResult.Hit;
                }
            }
            return ShotResult.Miss;
        }

        public bool WasShot(int row, int col)
        {
            foreach (DragShip ship in dragShips)
            {
                if (ship.Shots.Contains((row, col)))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsAllSunk()
        {
            return dragShips.All(ship => ship.IsSunk);
        }

        public int DeterminedSmallestShipSize()
        {
            return dragShips.Where(ship => !ship.IsSunk).Min(ship => (int)ship.type);
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
            foreach ((int, int) shot in shots)
            {
                boardState[shot.Item1, shot.Item2] = CellState.MISS;
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
