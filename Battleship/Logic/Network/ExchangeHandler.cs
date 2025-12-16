using Battleship.Logic.Game;
using Battleship.Logic.Network.Online;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using static Battleship.Logic.Network.Online.MQTTService;

namespace Battleship.Logic.Network
{
    public class ExchangeHandler
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


        private readonly string playerUsername;
        public string PlayerUsername { get { return playerUsername; } }


        private string? opponentUsername;
        public string? OpponentUsername { get { return opponentUsername; } }
        private int[] opponentShipData = [];
        public int[] OpponentShipData { get { return opponentShipData; } }


        private CancellationTokenSource? cts;
        private DateTime lastPingSend;
        private DateTime lastPingReceived;

        private bool isReady = false;

        public ExchangeHandler(string gameId, string password, string playerUsername, bool isHost)
        {
            Debug.WriteLine($"Exchange handler IsHost: {isHost}");
            this.mqttService = new MQTTService(gameId, password, playerUsername, isHost);
            this.mqttService.OnDataReceived += OnDataReceived;
            this.playerUsername = playerUsername;
            this.IsHost = isHost;
        }
        
        /// <summary>
        /// Start connection to service
        /// </summary>
        /// <returns>Task</returns>
        public async Task Connect()
        {
            await mqttService.Connect();

            if (!IsHost)
                SendConnect(playerUsername);
        }

        /// <summary>
        /// Process the received data
        /// </summary>
        /// <param name="category">The category of the data</param>
        /// <param name="data">The actual data</param>
        private async void OnDataReceived(PacketCategory category, string data)
        {
            Debug.WriteLine($"Received message for category {category}: {data}");
            try
            {
                string[] values = data.Split(',');
                Debug.WriteLine($"Values: {string.Join(", ", values)}");

                if (category == PacketCategory.PING)
                {
                    lastPingReceived = DateTime.Now;
                }
                else if (category == PacketCategory.CONNECT)
                {
                    string enemyUsername = values[0];
                    this.opponentUsername = enemyUsername;
                    OnConnect?.Invoke();
                    SendConnAck(playerUsername);
                    await StartPinger();
                }
                else if (category == PacketCategory.CONNACK)
                {
                    string enemyUsername = values[0];
                    this.opponentUsername = enemyUsername;
                    OnConnectAck?.Invoke();
                    await StartPinger();
                }
                else if (category == PacketCategory.SETTINGS)
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
                else if (category == PacketCategory.SETTINGSACK)
                {
                    OnSettingsAck?.Invoke();
                }
                else if (category == PacketCategory.READY)
                {
                    bool isEnemyReady = bool.Parse(values[0]);
                    opponentShipData = Array.ConvertAll<string, int>(values.Skip(1).ToArray(), s => int.Parse(s));
                    OnReady?.Invoke(isReady && isEnemyReady);
                    SendReadyAck();
                }
                else if (category == PacketCategory.READYACK)
                {
                    bool isEnemyReady = bool.Parse(values[0]);
                    OnReadyAck?.Invoke(isReady && isEnemyReady);
                }
                else if (category == PacketCategory.SHOOT)
                {
                    int row = int.Parse(values[0]);
                    int col = int.Parse(values[1]);
                    OnShoot?.Invoke(row, col);
                }
                else if (category == PacketCategory.MISS)
                {
                    int row = int.Parse(values[0]);
                    int col = int.Parse(values[1]);
                    OnMiss?.Invoke(row, col);
                }
                else if (category == PacketCategory.HIT)
                {
                    int row = int.Parse(values[0]);
                    int col = int.Parse(values[1]);
                    OnHit?.Invoke(row, col);
                }
                else if (category == PacketCategory.SUNK)
                {
                    int row = int.Parse(values[0]);
                    int col = int.Parse(values[1]);
                    OnSunk?.Invoke(row, col);
                }
                else if (category == PacketCategory.MESSAGE)
                {
                    int id = int.Parse(values[0]);
                    string message = String.Join(" ", values.Skip(1));
                    OnMessage?.Invoke(id, message);
                    SendMessageAck(id);
                }
                else if (category == PacketCategory.MESSAGEACK)
                {
                    int id = int.Parse(values[0]);
                    OnMessageAck?.Invoke(id);
                }
                else if (category == PacketCategory.DISCONNECT)
                {
                    OnDisconnect?.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        /// <summary>
        /// Start a task to frequently send a ping to the opponent.
        /// </summary>
        /// <returns>Task</returns>
        private async Task StartPinger()
        {
            // Set a cancellation token
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            // Set the last ping to now
            lastPingReceived = DateTime.Now;
            lastPingSend = DateTime.Now;

            await Task.Run(async () =>
            {
                Debug.WriteLine("Ping task started");
                try
                {
                    // Get the current datetime
                    DateTime now = DateTime.Now;
                    while (true)
                    {
                        // Check if the last received ping from opponent is older the 60 seconds
                        if (DateTime.Now - lastPingReceived > TimeSpan.FromSeconds(60))
                        {
                            Debug.WriteLine("Ping timeout");
                            // Invokes a timeout event when no ping is received from the opponent within 60 seconds
                            OnTimeout?.Invoke();
                            break;
                        }
                        // Send a ping to the opponent every 10 seconds to maintain the connection.
                        else if (DateTime.Now - lastPingSend > TimeSpan.FromSeconds(10))
                        {
                            // Send ping
                            SendPing();

                            // Update last sent ping
                            lastPingSend = DateTime.Now;
                        }
                        // To reduce performance load a delay of 10 seconds is implemented
                        await Task.Delay(100);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Unexpected error in ping task: " + e.Message);
                }
            }, token);

            Debug.WriteLine("Ping task stopped!");
        }

        /// <summary>
        /// Send ping request
        /// </summary>
        private async void SendPing()
        {
            await mqttService.Send(PacketCategory.PING, "PING");
        }

        /// <summary>
        /// Send connect packet
        /// </summary>
        /// <param name="username">The own selected username</param>
        public async void SendConnect(string username)
        {
            await mqttService.Send(PacketCategory.CONNECT, username);
        }

        /// <summary>
        /// Send connect acknowledge packet
        /// </summary>
        /// <param name="username">The own selected username</param>
        public async void SendConnAck(string username)
        {
            await mqttService.Send(PacketCategory.CONNACK, username);
        }

        /// <summary>
        /// Send settings packet
        /// </summary>
        /// <param name="size">The size of the board</param>
        /// <param name="hitBonus">An indicator to determine whether another shot can be fired after a hit.</param>
        /// <param name="restrictedArea">An indicator to determine whether a restricted area is displayed around each ship</param>
        /// <param name="carrierAmount">The amount of carrier ships</param>
        /// <param name="battleshipAmount">The amount of battle ships</param>
        /// <param name="cruiserAmount">The amount of crusier ships</param>
        /// <param name="submarineAmount">The amount of submarine ships</param>
        /// <param name="destroyerAmount">The amount of destroyer ships</param>
        public async void SendSettings(int boardSize, bool hitBonus, bool restrictedArea, int carrierAmount, int battleshipAmount, int cruiserAmount, int submarineAmount, int destroyerAmount)
        {
            string data = $"{boardSize},{hitBonus},{restrictedArea},{carrierAmount},{battleshipAmount},{cruiserAmount},{submarineAmount},{destroyerAmount}";
            await mqttService.Send(PacketCategory.SETTINGS, data);
        }

        /// <summary>
        /// Send settings acknowlegde packet
        /// </summary>
        public async void SendSettingsAck()
        {
            await mqttService.Send(PacketCategory.SETTINGSACK, "SETTINGSACK");
        }

        /// <summary>
        /// Send ready packet
        /// </summary>
        /// <param name="ships">The list of ship objects to be transmitted</param>
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

            await mqttService.Send(PacketCategory.READY, data);
        }

        /// <summary>
        /// Send ready acknowledge packet
        /// </summary>
        public async void SendReadyAck()
        {
            string data = $"{isReady}";
            await mqttService.Send(PacketCategory.READYACK, data);
        }

        /// <summary>
        /// Semd shoot packet
        /// </summary>
        /// <param name="row">The position in row</param>
        /// <param name="col">The position in col</param>
        public async void SendShoot(int row, int col)
        {
            string data = $"{row},{col}";
            await mqttService.Send(PacketCategory.SHOOT, data);
        }

        /// <summary>
        /// Send miss packet
        /// </summary>
        /// <param name="row">The position in row</param>
        /// <param name="col">The position in col</param>
        public async void SendMiss(int row, int col)
        {
            string data = $"{row},{col}";
            await mqttService.Send(PacketCategory.MISS, data);
        }

        /// <summary>
        /// Send hit packet
        /// </summary>
        /// <param name="row">The position in row</param>
        /// <param name="col">The position in col</param>
        public async void SendHit(int row, int col)
        {
            string data = $"{row},{col}";
            await mqttService.Send(PacketCategory.HIT, data);
        }

        /// <summary>
        /// Send sunk packet
        /// </summary>
        /// <param name="row">The position in row</param>
        /// <param name="col">The position in col</param>
        public async void SendSunk(int row, int col)
        {
            string data = $"{row},{col}";
            await mqttService.Send(PacketCategory.SUNK, data);
        }

        /// <summary>
        /// Send message packet
        /// </summary>
        /// <param name="id">Id of the message</param>
        /// <param name="message">The message to send</param>
        public async void SendMessage(int id, string message)
        {
            string data = $"{id},{message}";
            await mqttService.Send(PacketCategory.MESSAGE, data);
        }

        /// <summary>
        /// Send message acknowledge package
        /// </summary>
        /// <param name="id">Id of the message</param>
        public async void SendMessageAck(int id)
        {
            string data = $"{id}";
            await mqttService.Send(PacketCategory.MESSAGEACK, data);
        }

        /// <summary>
        /// Send disconnect packet
        /// </summary>
        /// <returns>Task</returns>
        public async Task SendDisconnect()
        {
            string data = "DISCONNECT";
            await mqttService.Send(PacketCategory.DISCONNECT, data);
        }


        /// <summary>
        /// Close connection to service, stop ping and remove all events
        /// </summary>
        /// <returns>Task</returns>
        public async Task Close()
        {
            await mqttService.Disconnect();
            cts?.Cancel();
            cts?.Dispose();
            OnTimeout = null;
            OnConnect = null;
            OnConnectAck = null;
            OnSettings = null;
            OnSettingsAck = null;
            OnReady = null;
            OnReadyAck = null;
            OnShoot = null;
            OnMiss = null;
            OnHit = null;
            OnSunk = null;
            OnMessage = null;
            OnMessageAck = null;
            OnDisconnect = null;
        }
    }
}
