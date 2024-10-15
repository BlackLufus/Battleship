using Battleship.Playground.ComputerLogic;
using Battleship.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Battleship.Playground
{
    public class GameLogic(GameSetting gameSetting, Playground enemyPlayground)
    {
        public delegate void ShotEventEventHandler(GameSetting gameSetting, Grid type, Playground playground);
        public event ShotEventEventHandler? ShotEvent;

        public delegate void GameEndedEventHandler();
        public event GameEndedEventHandler? GameEndedEvent;

        private readonly GameSetting gameSetting = gameSetting;
        private readonly Playground enemyPlayground = enemyPlayground;

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
                //try {
                    while (!gameEnded)
                    {
                        if (this.turn == turn)
                        {
                            Thread.Sleep(new Random().Next(75, 100));
                            bool hit = computerLogic.NextShot();
                            Debug.WriteLine("Hit: " + hit);
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
                        GameEndedEvent?.Invoke();
                    });
                /*}
                catch { }*/
            });
            thread.Start();
            thread.Name = "GameLogicThread";
            ThreadListener.AddThread(thread);
        }

        public void Shot(Grid playgroundGrid, Button enemyField)
        {
            if (gameSetting.GameMode != GameSetting.Mode.ComputerVsComputer && this.turn == TurnType.MyTurn)
            {
                int row = Grid.GetRow(enemyField);
                int col = Grid.GetColumn(enemyField);

                Playground.ShotResult shotResult = enemyPlayground.Shot(row, col);

                Debug.WriteLine("Shot at " + row + " " + col + " with result " + shotResult);

                ShotEvent?.Invoke(gameSetting, playgroundGrid, enemyPlayground);

                if (enemyPlayground.IsAllSunk())
                {
                    GameEndedEvent?.Invoke();
                }
                else if (!gameSetting.HitBonus || shotResult == Playground.ShotResult.Miss)
                {
                    this.turn = TurnType.EnemyTurn;
                }
            }
        }
    }
}
