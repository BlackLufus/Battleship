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
    public class GameLogic
    {
        public delegate void ShotEventEventHandler(GameSetting gameSetting, Grid type, Playground playground);
        public event ShotEventEventHandler? ShotEvent;

        public delegate void GameEndedEventHandler(TurnType turn);
        public event GameEndedEventHandler? GameEndedEvent;

        private readonly GameSetting gameSetting;
        private readonly Grid? myPlaygroundGrid;
        private readonly Playground? myPlayground;
        private readonly Grid? enemyPlaygroundGrid;
        private readonly Playground enemyPlayground;
        private readonly MQTTService? mqttService;

        public GameLogic(GameSetting gameSetting, Playground enemyPlayground)
        {
            this.gameSetting = gameSetting;
            this.enemyPlayground = enemyPlayground;
        }

        public GameLogic(GameSetting gameSetting, Grid myPlaygroundGrid, Playground myPlayground, Grid enemyPlaygroundGrid, Playground enemyPlayground, MQTTService mqttService)
        {
            this.gameSetting = gameSetting;
            this.myPlaygroundGrid = myPlaygroundGrid;
            this.myPlayground = myPlayground;
            this.enemyPlaygroundGrid = enemyPlaygroundGrid;
            this.enemyPlayground = enemyPlayground;
            this.mqttService = mqttService;

            turn = mqttService.IsMyTurn ? TurnType.MyTurn : TurnType.EnemyTurn;
            Debug.WriteLine(turn == TurnType.MyTurn ? "Ich bin dran!" : "Gegner ist dran!");

            this.mqttService.OnShoot += OnShoot;
            this.mqttService.OnHit += OnHit;
            this.mqttService.OnMiss += OnMiss;
            this.mqttService.OnSunk += OnSunk;
            this.mqttService.OnEndGame += OnEndGame;
            this.mqttService.OnDisconnected += OnUserDisconnected;
        }

        public enum TurnType
        {
            MyTurn = 0,
            EnemyTurn = 1
        }
        public TurnType Turn => turn;
        private TurnType turn = new Random().Next(0, 2) == 0 ? TurnType.MyTurn : TurnType.EnemyTurn;

        public void StartGame(Grid playgroundGrid, Playground playground, TurnType turn)
        {
            GameAI computerLogic = new(gameSetting, playground);
            bool gameEnded = false;
            Thread thread = new(() =>
            {
                try
                {
                    while (!gameEnded)
                    {
                        if (this.turn == turn)
                        {
                            Thread.Sleep(new Random().Next(75, 100));
                            bool hit = computerLogic.NextShot();
                            //Debug.WriteLine("Hit: " + hit);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ShotEvent?.Invoke(gameSetting, playgroundGrid, playground);
                            });
                            if (!gameSetting.HitBonus || !hit)
                            {
                                this.turn = this.turn == TurnType.MyTurn ? TurnType.EnemyTurn : TurnType.MyTurn;
                            }
                            if (playground.IsAllSunk())
                            {
                                gameEnded = true;
                            }
                        }
                        else
                        {
                            Thread.Sleep(75);
                        }
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GameEndedEvent?.Invoke(turn);
                    });
                }
                catch { }
            });
            thread.Start();
            thread.Name = "GameLogicThread";
            ThreadListener.AddThread(thread);
        }

        public void Shot(Grid playgroundGrid, Button enemyField)
        {
            if (gameSetting.GameMode == GameSetting.Mode.PlayerVsComputer && turn == TurnType.MyTurn)
            {
                int row = Grid.GetRow(enemyField);
                int col = Grid.GetColumn(enemyField);

                Playground.ShotResult shotResult = enemyPlayground.Shoot(row, col, gameSetting.RestrictedArea);

                //Debug.WriteLine("Shot at " + row + " " + col + " with result " + shotResult);

                ShotEvent?.Invoke(gameSetting, playgroundGrid, enemyPlayground);

                if (enemyPlayground.IsAllSunk())
                {
                    GameEndedEvent?.Invoke(TurnType.MyTurn);
                }
                else if (!gameSetting.HitBonus || shotResult == Playground.ShotResult.Miss)
                {
                    turn = TurnType.EnemyTurn;
                }
            }
            else if (gameSetting.GameMode == GameSetting.Mode.PlayerVsPlayer && turn == TurnType.MyTurn)
            {
                Debug.WriteLine("Shot at " + Grid.GetRow(enemyField) + " " + Grid.GetColumn(enemyField));
                int row = Grid.GetRow(enemyField);
                int col = Grid.GetColumn(enemyField);

                mqttService?.SendShoot(row, col);
            }
        }

        /// <summary>
        /// Handle shoot event
        /// </summary>
        /// <param name="row">row position</param>
        /// <param name="col">column position</param>
        private void OnShoot(int row, int col)
        {
            // Check if it's my turn and return if not
            if (turn == TurnType.MyTurn)
                return;

            // Get shot result
            Playground.ShotResult shotResult = myPlayground!.Shoot(row, col, gameSetting.RestrictedArea);

            // Update UI
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShotEvent?.Invoke(gameSetting, myPlaygroundGrid!, myPlayground!);
            });

            // Handle shot result
            if (shotResult == Playground.ShotResult.Miss)
            {
                // Send miss event to enemy
                mqttService?.SendMiss(row, col);
                turn = TurnType.MyTurn;
                Debug.WriteLine("Ich bin dran!");
            }
            else if (shotResult == Playground.ShotResult.Hit)
            {
                // Send hit event to enemy
                mqttService?.SendHit(row, col);

                // If hit bonus is disabled, it's my turn
                if (!gameSetting.HitBonus)
                {
                    turn = TurnType.MyTurn;
                    Debug.WriteLine("Ich bin dran!");
                }
            }
            else if (shotResult == Playground.ShotResult.Sunk)
            {
                // Send sunk event to enemy
                mqttService?.SendSunk(row, col);

                // Check if all ships are sunk
                if (myPlayground.IsAllSunk())
                {
                    // Send end game event to enemy
                    mqttService?.SendEndGame(mqttService.OpponentUsername!);

                    // Game ended
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GameEndedEvent?.Invoke(turn);
                    });

                    mqttService?.CloseConnection();
                }
                // If hit bonus is disabled, it's my turn
                else if (!gameSetting.HitBonus)
                {
                    turn = TurnType.MyTurn;
                    Debug.WriteLine("Ich bin dran!");
                }
            }
        }

        /// <summary>
        /// Handle hit event
        /// </summary>
        /// <param name="row">row position</param>
        /// <param name="col">column position</param>
        private void OnHit(int row, int col)
        {
            // Check if it's my turn and return if not
            if (turn != TurnType.MyTurn)
                return;

            // Define ship and shoot
            enemyPlayground.DefineShip(row, col);

            // Shoot
            enemyPlayground.Shoot(row, col, gameSetting.RestrictedArea);

            // Update UI
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShotEvent?.Invoke(gameSetting, enemyPlaygroundGrid!, enemyPlayground);
            });

            // If hit bonus is disabled, it's my turn
            if (!gameSetting.HitBonus)
            {
                turn = TurnType.EnemyTurn;
                Debug.WriteLine("Gegner ist dran!");
            }
        }

        /// <summary>
        /// Handle miss event
        /// </summary>
        /// <param name="row">row position</param>
        /// <param name="col">column position</param>
        private void OnMiss(int row, int col)
        {
            // Check if it's my turn and return if not
            if (turn != TurnType.MyTurn)
                return;

            // Shoot
            enemyPlayground.Shoot(row, col, false);

            // Update UI
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShotEvent?.Invoke(gameSetting, enemyPlaygroundGrid!, enemyPlayground);
            });

            // Enemy is now on turn
            turn = TurnType.EnemyTurn;
            Debug.WriteLine("Gegner ist dran!");
        }

        /// <summary>
        /// Handle sunk event
        /// </summary>
        /// <param name="row">row position</param>
        /// <param name="col">column position</param>
        private void OnSunk(int row, int col)
        {
            // Check if it's my turn and return if not
            if (turn != TurnType.MyTurn)
                return;

            // Define ship
            enemyPlayground.DefineShip(row, col, true);

            // Shoot
            enemyPlayground.Shoot(row, col, gameSetting.RestrictedArea);

            // Update UI
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShotEvent?.Invoke(gameSetting, enemyPlaygroundGrid!, enemyPlayground);
            });

            // If hit bonus is disabled, it's my turn
            if (!gameSetting.HitBonus)
            {
                turn = TurnType.EnemyTurn;
                Debug.WriteLine("Gegner ist dran!");
            }
        }

        /// <summary>
        /// Handle end game event
        /// </summary>
        /// <param name="winner"> winner username </param>
        private void OnEndGame(string winner)
        {
            Debug.WriteLine("Game ended. Winner: " + winner);
            // Game ended
            Application.Current.Dispatcher.Invoke(() =>
            {
                GameEndedEvent?.Invoke(turn);
            });
        }

        private void OnUserDisconnected()
        {
            Debug.WriteLine("User disconnected");
            // Game ended
            Application.Current.Dispatcher.Invoke(() =>
            {
                GameEndedEvent?.Invoke(TurnType.MyTurn);
            });
        }
    }
}
