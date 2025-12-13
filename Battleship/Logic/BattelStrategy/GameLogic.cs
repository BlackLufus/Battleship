using Battleship.Logic.BattelStrategy.Modes;
using Battleship.Logic.Global;
using Battleship.Logic.Network;
using Battleship.Logic.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Battleship.Logic.BattelStrategy
{
    public enum TurnType
    {
        MyTurn = 0,
        EnemyTurn = 1
    }

    public class GameLogic
    {
        public delegate void GameEndedEventHandler();
        public event GameEndedEventHandler? OnGameEndedEvent;

        public delegate void EnemyShotEventHandler();
        public event EnemyShotEventHandler? OnEnemyShotEvent;

        public delegate void FriendlyShotEventHandler();
        public event FriendlyShotEventHandler? OnFriendlyShotEvent;

        private readonly GameSetting gameSetting;

        private readonly PlaygroundBoardLogic enemyBoard;
        private readonly PlaygroundBoardLogic friendlyBoard;

        public TurnType turn = TurnType.EnemyTurn;

        public GameLogic(GameSetting gameSetting, PlaygroundBoardLogic enemyBoard, PlaygroundBoardLogic friendlyBoard)
        {
            this.gameSetting = gameSetting;
            this.enemyBoard = enemyBoard;
            this.friendlyBoard = friendlyBoard;
        }

        public async void Start()
        {
            GameAI computerLogic = new(gameSetting, friendlyBoard);
            bool gameEnded = false;

            Debug.WriteLine("Start");

            try
            {
                while (!gameEnded)
                {
                    if (this.turn == TurnType.EnemyTurn)
                    {
                        await Task.Delay(Random.Shared.Next(75, 100));

                        // ⬇️ schwere Berechnung im Hintergrund
                        bool hit = await Task.Run(() =>
                        {
                            return computerLogic.NextShot();
                        });

                        //Debug.WriteLine("Hit: " + hit);

                        // ⬇️ wieder UI-Thread
                        OnEnemyShotEvent?.Invoke();

                        if (!gameSetting.HitBonus || !hit)
                        {
                            this.turn = this.turn == TurnType.MyTurn
                                ? TurnType.EnemyTurn
                                : TurnType.MyTurn;
                        }

                        if (friendlyBoard.IsAllSunk())
                        {
                            gameEnded = true;
                        }
                    }
                    else
                    {
                        await Task.Delay(75);
                    }
                }

                OnGameEndedEvent?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void Shoot(int row, int col)
        {
            if (turn == TurnType.EnemyTurn)
                return;
            ShotResult result = enemyBoard.Shoot(row, col);
            if (result == ShotResult.Miss)
                turn = TurnType.EnemyTurn;
            OnFriendlyShotEvent?.Invoke();
        }
    }
}
