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
using static Battleship.Resources.Components.Dialog;

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

        private readonly ExchangeHandler? exchangeHandler;
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

        public GameLogic(ExchangeHandler exchangeHandler, GameSetting gameSetting, PlaygroundBoardLogic enemyBoard, PlaygroundBoardLogic friendlyBoard)
        {
            this.exchangeHandler = exchangeHandler;
            this.exchangeHandler.OnShoot += HandleShoot;
            this.exchangeHandler.OnMiss += HandleMiss;
            this.exchangeHandler.OnHit += HandleHit;
            this.exchangeHandler.OnSunk += HandleSunk;
            this.exchangeHandler.OnTimeout += HandleTimeout;
            this.exchangeHandler.OnDisconnect += HandleDisconnect;

            this.gameSetting = gameSetting;
            this.enemyBoard = enemyBoard;
            this.friendlyBoard = friendlyBoard;
        }

        public async void StartSinglePlayerMode()
        {
            GameAI computerLogic = new(gameSetting, friendlyBoard);
            bool gameEnded = false;

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
                        OnFriendlyShotEvent?.Invoke();

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

        public void StartMultiPlayerMode()
        {
            if (exchangeHandler != null)
                turn = exchangeHandler.IsHost ? TurnType.MyTurn : TurnType.EnemyTurn;
        }

        public void Shoot(int row, int col)
        {
            if (turn == TurnType.EnemyTurn)
                return;

            if (exchangeHandler == null)
            {
                ShotResult result = enemyBoard.Shoot(row, col);

                if (result == ShotResult.Miss)
                    turn = TurnType.EnemyTurn;

                OnEnemyShotEvent?.Invoke();
            }
            else
            {
                exchangeHandler.SendShoot(row, col);
            }
        }

        private void HandleShoot(int row, int col)
        {
            if (exchangeHandler == null)
                return;

            if (turn == TurnType.MyTurn)
                return;

            ShotResult result = friendlyBoard.Shoot(row, col);

            if (result == ShotResult.Miss)
            {
                turn = TurnType.MyTurn;
                exchangeHandler.SendMiss(row, col);
            }
            else if (result == ShotResult.Hit)
                exchangeHandler.SendHit(row, col);
            else if (result == ShotResult.Sunk)
                exchangeHandler.SendSunk(row, col);

            OnFriendlyShotEvent?.Invoke();

        }

        private void HandleMiss(int row, int col)
        {
            turn = TurnType.EnemyTurn;

            enemyBoard.Shoot(row, col, true);
            OnEnemyShotEvent?.Invoke();
        }

        private void HandleHit(int row, int col)
        {
            enemyBoard.Shoot(row, col, true);
            OnEnemyShotEvent?.Invoke();
        }

        private void HandleSunk(int row, int col)
        {
            enemyBoard.Shoot(row, col, true);
            OnEnemyShotEvent?.Invoke();
        }

        private void HandleTimeout()
        {

        }

        private void HandleDisconnect()
        {

        }
    }
}
