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
        public delegate void GameEndedEventHandler(bool hasWon);
        public event GameEndedEventHandler? OnGameEnded;

        public delegate void EnemyShotEventHandler();
        public event EnemyShotEventHandler? OnEnemyShot;

        public delegate void FriendlyShotEventHandler();
        public event FriendlyShotEventHandler? OnFriendlyShot;

        public delegate void TimeoutEventHandler();
        public event TimeoutEventHandler? OnTimeout;

        public delegate void DisconnectEventHandler();
        public event DisconnectEventHandler? OnDisconnect;

        private readonly ExchangeHandler? exchangeHandler;
        private readonly GameSetting gameSetting;

        private readonly PlaygroundBoardLogic enemyBoard;
        private readonly PlaygroundBoardLogic friendlyBoard;

        public TurnType turn = TurnType.EnemyTurn;
        private CancellationTokenSource? cts;

        public GameLogic(GameSetting gameSetting, PlaygroundBoardLogic enemyBoard, PlaygroundBoardLogic friendlyBoard)
        {
            // Init basic variables
            this.gameSetting = gameSetting;
            this.enemyBoard = enemyBoard;
            this.friendlyBoard = friendlyBoard;
        }

        public GameLogic(ExchangeHandler exchangeHandler, GameSetting gameSetting, PlaygroundBoardLogic enemyBoard, PlaygroundBoardLogic friendlyBoard)
        {
            // Init exchange handler
            this.exchangeHandler = exchangeHandler;
            this.exchangeHandler.OnShoot += HandleShoot;
            this.exchangeHandler.OnMiss += HandleMiss;
            this.exchangeHandler.OnHit += HandleHit;
            this.exchangeHandler.OnSunk += HandleSunk;
            this.exchangeHandler.OnTimeout += HandleTimeout;
            this.exchangeHandler.OnDisconnect += HandleDisconnect;

            // Init basic variables
            this.gameSetting = gameSetting;
            this.enemyBoard = enemyBoard;
            this.friendlyBoard = friendlyBoard;
        }

        public async void StartSinglePlayerMode()
        {
            // Ignore when exchange handler is not null
            if (exchangeHandler != null)
                return;

            // Init enemy computer logic
            GameAI enemyComputerLogic = new(gameSetting, friendlyBoard);

            // Init friendly computer logic when mode is computer vs computer
            GameAI? friendlyComputerLogic = null;
            if (gameSetting.GameMode == GameSetting.Mode.ComputerVsComputer)
                friendlyComputerLogic = new GameAI(gameSetting, enemyBoard);

            // Init cancellation token
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            // Run task
            await Task.Run(async () =>
            {
                while (true)
                {
                    // Procedure when turn is enemy turn
                    if (this.turn == TurnType.EnemyTurn)
                    {
                        // Delay 250 to 750 ms
                        //await Task.Delay(Random.Shared.Next(250, 750));

                        // Select next shoot and return hit
                        bool hit = await Task.Run(() =>
                        {
                            return enemyComputerLogic.NextShot();
                        });

                        // Update UI (friendly board)
                        OnFriendlyShot?.Invoke();

                        // Change turn when hit bonus is disabled
                        if (!gameSetting.HitBonus || !hit)
                            this.turn = TurnType.MyTurn;

                        // Exit loop when all ships are sunk
                        if (friendlyBoard.IsAllSunk())
                        {
                            OnGameEnded?.Invoke(false);
                            break;
                        }
                    }
                    // Procedure when turn is my turn
                    else if (friendlyComputerLogic != null && this.turn == TurnType.MyTurn)
                    {
                        // Delay 250 to 750 ms
                        //await Task.Delay(Random.Shared.Next(250, 750));

                        // Select next shoot and return hit
                        bool hit = await Task.Run(() =>
                        {
                            return friendlyComputerLogic.NextShot();
                        });

                        // Update UI (friendly board)
                        OnEnemyShot?.Invoke();

                        // Change turn when hit bonus is disabled
                        if (!gameSetting.HitBonus || !hit)
                            this.turn = TurnType.EnemyTurn;

                        // Exit loop when all ships are sunk
                        if (enemyBoard.IsAllSunk())
                        {
                            OnGameEnded?.Invoke(true);
                            break;
                        }
                    }
                    // To decrease performance issues a delay of 1 ms is set
                    await Task.Delay(1);
                }
                cts.Cancel();
                cts.Dispose();
            }, token);
        }

        public void StartMultiPlayerMode()
        {
            // Ignore when exchange handler is null
            if (exchangeHandler == null)
                return;

            // Choose turn
            turn = exchangeHandler.IsHost ? TurnType.MyTurn : TurnType.EnemyTurn;
        }

        public void Shoot(int row, int col)
        {
            // Ignore shoot when gamemode is computer vs computer
            if (gameSetting.GameMode == GameSetting.Mode.ComputerVsComputer)
                return;

            // Ignore shoot when turn is enemy turn
            if (turn == TurnType.EnemyTurn)
                return;

            if (exchangeHandler == null)
            {
                ShotResult result = enemyBoard.Shoot(row, col);

                // When result is MISS change turn tu enemy turn
                if (result == ShotResult.Miss)
                    turn = TurnType.EnemyTurn;

                // Update UI
                OnEnemyShot?.Invoke();

                // Close exchange handler, when all ships are sunk
                if (enemyBoard.IsAllSunk())
                {
                    cts?.Cancel();
                    OnGameEnded?.Invoke(true);
                }
            }
            else
            {
                // Send shoot
                exchangeHandler.SendShoot(row, col);
            }
        }

        private async void HandleShoot(int row, int col)
        {
            // Ignore when exchange handler is null
            if (exchangeHandler == null)
                return;

            // Ignore Shoot when turn is my turn
            if (turn == TurnType.MyTurn)
                return;

            // Get shot result
            ShotResult result = friendlyBoard.Shoot(row, col);

            // Result is MISS
            if (result == ShotResult.Miss)
            {
                turn = TurnType.MyTurn;
                exchangeHandler.SendMiss(row, col);
            }
            // Result is HIT
            else if (result == ShotResult.Hit)
            {
                exchangeHandler.SendHit(row, col);
            }
            // Result is SUNK
            else if (result == ShotResult.Sunk)
            {
                exchangeHandler.SendSunk(row, col);
            }

            // Update UI (friendly board)
            OnFriendlyShot?.Invoke();

            // Close exchange handler, when all ships are sunk
            if (friendlyBoard.IsAllSunk())
            {
                OnGameEnded?.Invoke(false);
                await exchangeHandler.Close();
            }
        }

        private void HandleMiss(int row, int col)
        {
            // Ignore when exchange handler is null
            if (exchangeHandler == null)
                return;

            // Change turn to enemy turn
            turn = TurnType.EnemyTurn;

            // Send Miss
            enemyBoard.Shoot(row, col);

            // Update UI (enemy board)
            OnEnemyShot?.Invoke();
        }

        private void HandleHit(int row, int col)
        {
            // Ignore when exchange handler is null
            if (exchangeHandler == null)
                return;

            // Semd Hit
            enemyBoard.Shoot(row, col);

            // Update UI (enemy board)
            OnEnemyShot?.Invoke();
        }

        private async void HandleSunk(int row, int col)
        {
            // Ignore when exchange handler is null
            if (exchangeHandler == null)
                return;

            // Send Sunk
            enemyBoard.Shoot(row, col);

            // Update UI (enemy board)
            OnEnemyShot?.Invoke();

            // Close exchange handler, when all ships are sunk
            if (enemyBoard.IsAllSunk())
            {
                // Send Game Ended event
                OnGameEnded?.Invoke(true);

                // Close exchange handler
                await exchangeHandler.Close();
            }
        }

        private async void HandleTimeout()
        {
            // Close exchange handler
            if (exchangeHandler != null)
                await exchangeHandler.Close();

            // Send Timeout event
            OnTimeout?.Invoke();
        }

        private async void HandleDisconnect()
        {
            // Close exchange handler
            if (exchangeHandler != null)
                await exchangeHandler.Close();

            // Send Disconnect event
            OnDisconnect?.Invoke();
        }
    }
}
