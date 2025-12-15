using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using static Battleship.Logic.Network.MQTTService;

namespace Battleship.Logic.Network
{
    public class ExchangeHandler : IDisposable
    {
        public delegate void TimeoutDelegate();
        public event TimeoutDelegate? OnTimeout;

        public delegate void ConnectDelegate();
        public event ConnectDelegate? OnConnect;

        public delegate void ConnectAckDelegate();
        public event ConnectAckDelegate? OnConnectAck;

        public delegate void SettingsDelegate(int size, bool hitBonus, bool restrictedArea, int carrierAmount, int battleshipAmount, int cruiserAmount, int submarineAmount, int destroyerAmount);
        public event SettingsDelegate? OnSettings;

        public delegate void SettingsAckDelegate();
        public event SettingsAckDelegate? OnSettingsAck;

        public delegate void ReadyDelegate(bool readyToStart);
        public event ReadyDelegate? OnReady;

        public delegate void ReadyAckDelegate(bool readyToStart);
        public event ReadyAckDelegate? OnReadyAck;

        public delegate void ShootDelegate(int x, int y);
        public event ShootDelegate? OnShoot;

        public delegate void MissDelegate(int x, int y);
        public event MissDelegate? OnMiss;

        public delegate void HitDelegate(int x, int y);
        public event HitDelegate? OnHit;

        public delegate void SunkDelegate(int x, int y);
        public event SunkDelegate? OnSunk;

        public delegate void MessageDelegate(int id, string message);
        public event MessageDelegate? OnMessage;

        public delegate void MessageAckDelegate(int id);
        public event MessageAckDelegate? OnMessageAck;

        public delegate void DisconnectDelegate();
        public event DisconnectDelegate? OnDisconnect;


        private readonly MQTTService mqttService;
        public readonly bool IsHost;


        private readonly string username;
        public string Username { get { return username; } }


        private string? enemyUsername;
        public string? EnemyUsername { get { return enemyUsername; } }
        private int[] enemyShipData = [];
        public int[] EnemyShipData { get { return enemyShipData; } }


        private CancellationTokenSource? cts;
        private DateTime lastPingSend;
        private DateTime lastPingReceived;


        private bool isReady = false;

        public ExchangeHandler(string gameId, string password, string username, bool isHost)
        {
            Debug.WriteLine($"Exchange handler IsHost: {isHost}");
            this.mqttService = new MQTTService(gameId, password, username, isHost);
            this.mqttService.OnDataReceived += OnDataReceived;
            this.username = username;
            this.IsHost = isHost;
        }

        public async Task Connect()
        {
            await mqttService.Connect();

            if (!IsHost)
                SendConnect(username);

            //await StartPinger();
        }

        private async void OnDataReceived(TopicCategory category, string data)
        {
            Debug.WriteLine($"Received message for category {category}: {data}");
            //try
            //{
            string[] values = data.Split(',');
            Debug.WriteLine($"Values: {string.Join(", ", values)}");

            if (category == TopicCategory.PING)
            {
                lastPingReceived = DateTime.Now;
            }
            else if (category == TopicCategory.CONNECT)
            {
                string enemyUsername = values[0];
                this.enemyUsername = enemyUsername;
                OnConnect?.Invoke();
                SendConnAck(username);
                await StartPinger();
            }
            else if (category == TopicCategory.CONNACK)
            {
                string enemyUsername = values[0];
                this.enemyUsername = enemyUsername;
                OnConnectAck?.Invoke();
                await StartPinger();
            }
            else if (category == TopicCategory.SETTINGS)
            {
                int size = int.Parse(values[0]);
                bool hitBonus = bool.Parse(values[1]);
                bool restrictedArea = bool.Parse(values[2]);
                int carrierAmount = int.Parse(values[3]);
                int battleshipAmount = int.Parse(values[4]);
                int cruiserAmount = int.Parse(values[5]);
                int submarineAmount = int.Parse(values[6]);
                int destroyerAmount = int.Parse(values[7]);
                OnSettings?.Invoke(size, hitBonus, restrictedArea, carrierAmount, battleshipAmount, cruiserAmount, submarineAmount, destroyerAmount);
                SendSettingsAck();
            }
            else if (category == TopicCategory.SETTINGSACK)
            {
                OnSettingsAck?.Invoke();
            }
            else if (category == TopicCategory.READY)
            {
                bool isEnemyReady = bool.Parse(values[0]);
                enemyShipData = Array.ConvertAll<string, int>(values.Skip(1).ToArray(), s => int.Parse(s));
                OnReady?.Invoke(isReady && isEnemyReady);
                SendReadyAck();
            }
            else if (category == TopicCategory.READYACK)
            {
                bool isEnemyReady = bool.Parse(values[0]);
                OnReadyAck?.Invoke(isReady && isEnemyReady);
            }
            else if (category == TopicCategory.SHOOT)
            {
                int row = int.Parse(values[0]);
                int col = int.Parse(values[1]);
                OnShoot?.Invoke(row, col);
            }
            else if (category == TopicCategory.MISS)
            {
                int row = int.Parse(values[0]);
                int col = int.Parse(values[1]);
                OnMiss?.Invoke(row, col);
            }
            else if (category == TopicCategory.HIT)
            {
                int row = int.Parse(values[0]);
                int col = int.Parse(values[1]);
                OnHit?.Invoke(row, col);
            }
            else if (category == TopicCategory.SUNK)
            {
                int row = int.Parse(values[0]);
                int col = int.Parse(values[1]);
                OnSunk?.Invoke(row, col);
            }
            else if (category == TopicCategory.MESSAGE)
            {
                int id = int.Parse(values[0]);
                string message = String.Join(" ", values.Skip(1));
                OnMessage?.Invoke(id, message);
                SendMessageAck(id);
            }
            else if (category == TopicCategory.MESSAGEACK)
            {
                int id = int.Parse(values[0]);
                OnMessageAck?.Invoke(id);
            }
            else if (category == TopicCategory.DISCONNECT)
            {
                OnDisconnect?.Invoke();
            }
            //}
            //catch (Exception e) 
            //{
            //    Debug.WriteLine(e.Message);
            //    Debug.WriteLine(e.StackTrace);
            //}
        }

        private async Task StartPinger()
        {
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            lastPingReceived = DateTime.Now;
            lastPingSend = DateTime.Now;

            await Task.Run(async () =>
            {
                Debug.WriteLine("Ping task started");
                try
                {
                    DateTime now = DateTime.Now;
                    while (true)
                    {
                        if (DateTime.Now - lastPingReceived > TimeSpan.FromSeconds(60))
                        {
                            Debug.WriteLine("Ping timeout");
                            OnTimeout?.Invoke();
                            break;
                        }
                        else if (DateTime.Now - lastPingSend > TimeSpan.FromSeconds(10))
                        {
                            SendPing();
                            lastPingSend = DateTime.Now;
                        }
                        await Task.Delay(100);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Unexpected error in ping task: " + e.Message);
                }
            }, token);
        }

        private async void SendPing()
        {
            await mqttService.Send(TopicCategory.PING, "PING");
        }

        public async void SendConnect(string username)
        {
            await mqttService.Send(TopicCategory.CONNECT, username);
        }

        public async void SendConnAck(string username)
        {
            await mqttService.Send(TopicCategory.CONNACK, username);
        }

        public async void SendSettings(int size, bool hitBonus, bool restrictedArea, int carrierAmount, int battleshipAmount, int cruiserAmount, int submarineAmount, int destroyerAmount)
        {
            string data = $"{size},{hitBonus},{restrictedArea},{carrierAmount},{battleshipAmount},{cruiserAmount},{submarineAmount},{destroyerAmount}";
            await mqttService.Send(TopicCategory.SETTINGS, data);
        }

        public async void SendSettingsAck()
        {
            await mqttService.Send(TopicCategory.SETTINGSACK, "SETTINGSACK");
        }

        public async void SendReady(List<Ship> ships)
        {
            isReady = !isReady;
            List<string> dataList = [isReady.ToString()];
            foreach (Ship ship in ships)
            {
                dataList.Add(((int)ship.type).ToString());
                dataList.Add(((int)ship.orientation).ToString());
                dataList.Add(ship.row.ToString());
                dataList.Add(ship.col.ToString());
            }

            string data = String.Join(",", dataList.ToArray());

            await mqttService.Send(TopicCategory.READY, data);
        }

        public async void SendReadyAck()
        {
            string data = $"{isReady}";
            await mqttService.Send(TopicCategory.READYACK, data);
        }

        public async void SendShoot(int row, int col)
        {
            string data = $"{row},{col}";
            await mqttService.Send(TopicCategory.SHOOT, data);
        }

        public async void SendMiss(int row, int col)
        {
            string data = $"{row},{col}";
            await mqttService.Send(TopicCategory.MISS, data);
        }

        public async void SendHit(int row, int col)
        {
            string data = $"{row},{col}";
            await mqttService.Send(TopicCategory.HIT, data);
        }

        public async void SendSunk(int row, int col)
        {
            string data = $"{row},{col}";
            await mqttService.Send(TopicCategory.SUNK, data);
        }

        public async void SendMessage(int id, string message)
        {
            string data = $"{id},{message}";
            await mqttService.Send(TopicCategory.MESSAGE, data);
        }

        public async void SendMessageAck(int id)
        {
            string data = $"{id}";
            await mqttService.Send(TopicCategory.MESSAGEACK, data);
        }

        public async void SendDisconnect()
        {
            string data = "DISCONNECT";
            await mqttService.Send(TopicCategory.DISCONNECT, data);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
