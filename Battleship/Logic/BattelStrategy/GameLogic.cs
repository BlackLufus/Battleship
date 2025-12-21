using Battleship.Logic.BattelStrategy.Modes;
using Battleship.Logic.Game;
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
        Player = 0,
        Opponent = 1
    }

    public class GameLogic
    {
        public delegate void GameEndedEventHandler(bool hasWon);
        public event GameEndedEventHandler? OnGameEnded;

        public delegate void EnemyShotEventHandler();
        public event EnemyShotEventHandler? OnShotAtOpponentBoard;

        public delegate void FriendlyShotEventHandler();
        public event FriendlyShotEventHandler? OnShotAtPlayerBoard;

        public delegate void TimeoutEventHandler();
        public event TimeoutEventHandler? OnTimeout;

        public delegate void DisconnectEventHandler();
        public event DisconnectEventHandler? OnDisconnect;

        private readonly ExchangeHandler? exchangeHandler;
        private readonly GameSetting gameSetting;

        private readonly PlaygroundBoardLogic opponentBoard;
        private readonly PlaygroundBoardLogic playerBoard;

        public TurnType turn = TurnType.Opponent;
        private CancellationTokenSource? cts;

        /// <summary>
        /// Init Game Logic
        /// </summary>
        /// <param name="exchangeHandler">The exchange handler</param>
        /// <param name="gameSetting">Game settings</param>
        /// <param name="opponentBoard">The enemey board</param>
        /// <param name="allyBoard">The friendly board</param>
        public GameLogic(GameSetting gameSetting, PlaygroundBoardLogic opponentBoard, PlaygroundBoardLogic playerBoard)
        {
            // Init basic variables
            this.gameSetting = gameSetting;
            this.opponentBoard = opponentBoard;
            this.playerBoard = playerBoard;
        }

        /// <summary>
        /// Init Game Logic with exchange handler
        /// </summary>
        /// <param name="exchangeHandler">The exchange handler</param>
        /// <param name="gameSetting">Game settings</param>
        /// <param name="opponentBoard">The enemey board</param>
        /// <param name="playerBoard">The friendly board</param>
        public GameLogic(ExchangeHandler exchangeHandler, GameSetting gameSetting, PlaygroundBoardLogic opponentBoard, PlaygroundBoardLogic playerBoard)
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
            this.opponentBoard = opponentBoard;
            this.playerBoard = playerBoard;
        }

        /// <summary>
        /// Start singleplayer mode
        /// </summary>
        public async void StartSinglePlayerMode()
        {
            // Ignore when exchange handler is not null
            if (exchangeHandler != null)
                return;

            // Init enemy computer logic
            GameAI enemyComputerLogic = new(gameSetting, playerBoard);

            // Init friendly computer logic when mode is computer vs computer
            GameAI? friendlyComputerLogic = null;
            if (gameSetting.GameMode == GameSetting.Mode.ComputerVsComputer)
                friendlyComputerLogic = new GameAI(gameSetting, opponentBoard);

            // Init cancellation token
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            // Run task
            await Task.Run(async () =>
            {
                while (true)
                {
                    // Procedure when turn is enemy turn
                    if (this.turn == TurnType.Opponent)
                    {
                        // Delay 250 to 750 ms
                        await Task.Delay(Random.Shared.Next(250, 750));

                        // Select next shoot and return hit
                        bool hit = await Task.Run(() =>
                        {
                            return enemyComputerLogic.NextShot();
                        });

                        // Change turn when hit bonus is disabled
                        if (!gameSetting.HitBonus || !hit)
                            this.turn = TurnType.Player;

                        // Update UI (friendly board)
                        OnShotAtPlayerBoard?.Invoke();

                        // Exit loop when all ships are sunk
                        if (playerBoard.IsAllSunk())
                        {
                            OnGameEnded?.Invoke(false);
                            break;
                        }
                    }
                    // Procedure when turn is my turn
                    else if (friendlyComputerLogic != null && this.turn == TurnType.Player)
                    {
                        // Delay 250 to 750 ms
                        await Task.Delay(Random.Shared.Next(250, 750));

                        // Select next shoot and return hit
                        bool hit = await Task.Run(() =>
                        {
                            return friendlyComputerLogic.NextShot();
                        });

                        // Change turn when hit bonus is disabled
                        if (!gameSetting.HitBonus || !hit)
                            this.turn = TurnType.Opponent;

                        // Update UI (friendly board)
                        OnShotAtOpponentBoard?.Invoke();

                        // Exit loop when all ships are sunk
                        if (opponentBoard.IsAllSunk())
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

        /// <summary>
        /// Start multiplayer mode
        /// </summary>
        public void StartMultiPlayerMode()
        {
            // Ignore when exchange handler is null
            if (exchangeHandler == null)
                return;

            // Choose turn
            turn = exchangeHandler.IsHost ? TurnType.Player : TurnType.Opponent;
        }

        /// <summary>
        /// Handle shoot to enemy board
        /// </summary>
        /// <param name="row">Row position</param>
        /// <param name="col">Col poisition</param>
        public void Shoot(int row, int col)
        {
            // Ignore shoot when gamemode is computer vs computer
            if (gameSetting.GameMode == GameSetting.Mode.ComputerVsComputer)
                return;

            // Ignore shoot when turn is enemy turn
            if (turn == TurnType.Opponent)
                return;

            if (exchangeHandler == null)
            {
                ShotResult result = opponentBoard.Shoot(row, col);

                // When result is MISS change turn tu enemy turn
                if (!gameSetting.HitBonus || result == ShotResult.Miss)
                    turn = TurnType.Opponent;

                // Update UI
                OnShotAtOpponentBoard?.Invoke();

                // Close exchange handler, when all ships are sunk
                if (opponentBoard.IsAllSunk())
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

        /// <summary>
        /// Handle incoming shoot from enemy (to friendly board)
        /// </summary>
        /// <param name="row">Row position</param>
        /// <param name="col">Col poisition</param>
        private async void HandleShoot(int row, int col)
        {
            // Ignore when exchange handler is null
            if (exchangeHandler == null)
                return;

            // Ignore Shoot when turn is my turn
            if (turn == TurnType.Player)
                return;

            // Get shot result
            ShotResult result = playerBoard.Shoot(row, col);

            // Result is MISS
            if (result == ShotResult.Miss)
            {
                turn = TurnType.Player;
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
            OnShotAtPlayerBoard?.Invoke();

            // Close exchange handler, when all ships are sunk
            if (playerBoard.IsAllSunk())
            {
                OnGameEnded?.Invoke(false);
                await exchangeHandler.Close();
            }
        }

        /// <summary>
        /// Handle miss result from enemy
        /// </summary>
        /// <param name="row">Row position</param>
        /// <param name="col">Col poisition</param>
        private void HandleMiss(int row, int col)
        {
            // Ignore when exchange handler is null
            if (exchangeHandler == null)
                return;

            // Change turn to enemy turn
            turn = TurnType.Opponent;

            // Send Miss
            opponentBoard.Shoot(row, col);

            // Update UI (enemy board)
            OnShotAtOpponentBoard?.Invoke();
        }

        /// <summary>
        /// Handle hit result from enemy
        /// </summary>
        /// <param name="row">Row position</param>
        /// <param name="col">Col poisition</param>
        private void HandleHit(int row, int col)
        {
            // Ignore when exchange handler is null
            if (exchangeHandler == null)
                return;

            // Semd Hit
            opponentBoard.Shoot(row, col);

            // Update UI (enemy board)
            OnShotAtOpponentBoard?.Invoke();
        }

        /// <summary>
        /// Handel sunk result from enemy board
        /// </summary>
        /// <param name="row">Row position</param>
        /// <param name="col">Col poisition</param>
        private async void HandleSunk(int row, int col)
        {
            // Ignore when exchange handler is null
            if (exchangeHandler == null)
                return;

            // Send Sunk
            opponentBoard.Shoot(row, col);

            // Update UI (enemy board)
            OnShotAtOpponentBoard?.Invoke();

            // Close exchange handler, when all ships are sunk
            if (opponentBoard.IsAllSunk())
            {
                // Send Game Ended event
                OnGameEnded?.Invoke(true);

                // Close exchange handler
                await exchangeHandler.Close();
            }
        }

        /// <summary>
        /// Handle timout event
        /// </summary>
        private async void HandleTimeout()
        {
            // Close exchange handler
            if (exchangeHandler != null)
                await exchangeHandler.Close();

            // Send Timeout event
            OnTimeout?.Invoke();
        }

        /// <summary>
        /// Handle disconnect event
        /// </summary>
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
