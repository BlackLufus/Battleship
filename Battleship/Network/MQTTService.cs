using Battelship.Lobby;
using Battelship;
using Battleship.Global;
using Battleship.Lobby;
using mqtt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace Battleship.Network
{
    public class MQTTService
    {
        private readonly string address = "broker-cn.emqx.io";
        private readonly int port = 1883;

        private CancellationTokenSource? cts;
        private Task? pingTask;
        private DateTime lastPing;

        private string topic;

        private readonly Mqtt client;
        public Mqtt Client => client;

        private readonly string username;
        private readonly string password;

        public readonly bool IsHost;
        private bool isReady = false;
        public bool IsMyTurn = false;//new Random().Next(0, 2) == 0;

        private string? opponentUsername = null;
        public string OpponentUsername => opponentUsername ?? "";
        public bool IsOpponentConnected => opponentUsername != null;
        private bool isOpponentReady = false;
        public bool IsOpponentReady => isOpponentReady;

        // Event Types for MQTT and their values
        public enum EventType
        {
            Ping, // [ping]
            Connect, // [Username]
            Accept, // [Username]
            Disconnect, // []
            GameSettings, // [Size, HitBonus, RestrictedArea, BattleshipAmount, CruiserAmount, SubmarineAmount, DestroyerAmount, CarrierAmount]
            Ready, // []
            StartGame, // [Turn]
            Shoot, // [X, Y]
            Hit, // [X, Y]
            Miss, // [X, Y]
            Sunk, // [X, Y]
            EndGame, // [Winner]
            ChatMessage // [Message]
        }

        public delegate void ConnectionTimeoutDelegate();
        public event ConnectionTimeoutDelegate? OnConnectionTimeout;

        public delegate void OnConnectionSuccessDelegate();
        public event OnConnectionSuccessDelegate? OnConnectionSuccess;

        public delegate void ClientConnectedDelegate();
        public event ClientConnectedDelegate? OnClientConnected;

        public delegate void DisconnectedDelegate();
        public event DisconnectedDelegate? OnDisconnected;

        public delegate void GameSettingsDelegate(int size, bool hitBonus, bool restrictedArea, int carrierAmount, int battleshipAmount, int cruiserAmount, int submarineAmount, int destroyerAmount);
        public event GameSettingsDelegate? OnGameSettings;

        public delegate void StartGameDelegate();
        public event StartGameDelegate? OnStartGame;

        public delegate void ShootDelegate(int x, int y);
        public event ShootDelegate? OnShoot;

        public delegate void HitDelegate(int x, int y);
        public event HitDelegate? OnHit;

        public delegate void MissDelegate(int x, int y);
        public event MissDelegate? OnMiss;

        public delegate void SunkDelegate(int x, int y);
        public event SunkDelegate? OnSunk;

        public delegate void EndGameDelegate(string winner);
        public event EndGameDelegate? OnEndGame;

        public delegate void ChatMessageDelegate(string message);
        public event ChatMessageDelegate? OnChatMessage;

        public MQTTService(string gameID, string password, string username, bool isHost)
        {
            client = new Mqtt();
            client.MessageReceived += OnMessage;
            this.lastPing = DateTime.Now;
            this.topic = "battleship/" + gameID;
            this.password = password;
            this.username = username;
            this.IsHost = isHost;
            this.IsMyTurn = isHost;
            StartConnection();
        }

        private async void StartConnection()
        {
            await client.Connect(this.address, this.port, this.username);
            if (IsHost)
            {
                await client.Subscribe(this.topic + "/client/#");
                this.topic += "/host";
            }
            if (!IsHost)
            {
                // subscribe to 
                await client.Subscribe(this.topic + "/host/#");
                this.topic += "/client";
                await client.Publish(this.topic + "/Connect", username);
            }
        }

        private async Task CheckPing()
        {
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            await Task.Run(async () =>
            {
                Debug.WriteLine("Ping task started");
                try
                {
                    DateTime lastPingSend = DateTime.Now;
                    while (true)
                    {
                        if (DateTime.Now - lastPing > TimeSpan.FromSeconds(20))
                        {
                            Debug.WriteLine("Ping timeout");
                            OnDisconnected?.Invoke();
                            CloseConnection();
                            break;
                        }
                        if (DateTime.Now - lastPingSend > TimeSpan.FromSeconds(5))
                        {
                            try
                            {
                                await client.Publish(this.topic + "/Ping", "ping");
                                lastPingSend = DateTime.Now;
                            }
                            catch (IOException e)
                            {
                                Debug.WriteLine("Network error during ping: " + e.Message);
                                OnDisconnected?.Invoke();
                                CloseConnection();
                                break;
                            }
                        }
                        await Task.Delay(1000);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Unexpected error in ping task: " + e.Message);
                }
            }, token);
        }

        private async Task CheckConnection()
        {
            await Task.Run(async () =>
            {
                DateTime startTime = DateTime.Now;
                while (true)
                {
                    if (DateTime.Now - startTime > TimeSpan.FromSeconds(3))
                    {
                        Debug.WriteLine("Connection Timeout!");
                        OnConnectionTimeout?.Invoke();
                        CloseConnection();
                        break;
                    }
                    await Task.Delay(1000);
                }
            });
        }

        private async void OnMessage(string topic, string message)
        {
            Debug.WriteLine($"Received message on topic {topic}: {message}");
            try
            {
                EventType eventType = (EventType)Enum.Parse(typeof(EventType), topic.Split('/').Last());
                Debug.WriteLine($"Event Type: {eventType}");
                string[] values = message.Split(',');
                Debug.WriteLine($"Values: {string.Join(", ", values)}");

                switch (eventType)
                {
                    case EventType.Ping:
                        lastPing = DateTime.Now;
                        break;
                    case EventType.Connect:
                        Debug.WriteLine("Is Enemy Connected: " + IsOpponentConnected);
                        if (!IsOpponentConnected)
                        {
                            Debug.WriteLine("Enemy connected");
                            opponentUsername = values[0];
                            OnClientConnected?.Invoke();
                            await client.Publish(this.topic + "/Accept", this.username);
                            CheckPing();
                        }
                        break;
                    case EventType.Accept:
                        if (!IsOpponentConnected)
                        {
                            opponentUsername = values[0];
                            OnConnectionSuccess?.Invoke();
                            CheckPing();
                        }
                        break;
                    case EventType.Disconnect:
                        OnDisconnected?.Invoke();
                        CloseConnection();
                        break;
                    case EventType.GameSettings:
                        OnGameSettings?.Invoke(int.Parse(values[0]), bool.Parse(values[1]), bool.Parse(values[2]), int.Parse(values[3]), int.Parse(values[4]), int.Parse(values[5]), int.Parse(values[6]), int.Parse(values[7]));
                        break;
                    case EventType.Ready:
                        isOpponentReady = true;
                        CheckReadyState();
                        break;
                    case EventType.StartGame:
                        Debug.WriteLine("I start the game: " + IsMyTurn);
                        OnStartGame?.Invoke();
                        break;
                    case EventType.Shoot:
                        OnShoot?.Invoke(int.Parse(values[0]), int.Parse(values[1]));
                        break;
                    case EventType.Hit:
                        OnHit?.Invoke(int.Parse(values[0]), int.Parse(values[1]));
                        break;
                    case EventType.Miss:
                        OnMiss?.Invoke(int.Parse(values[0]), int.Parse(values[1]));
                        break;
                    case EventType.Sunk:
                        OnSunk?.Invoke(int.Parse(values[0]), int.Parse(values[1]));
                        break;
                    case EventType.EndGame:
                        OnEndGame?.Invoke(values[0]);
                        break;
                    case EventType.ChatMessage:
                        OnChatMessage?.Invoke(message);
                        CloseConnection();
                        break;
                    default:
                        break;
                }
            }
            catch { }
        }

        public async void SendGameSettings(int size, bool hitBonus, bool restrictedArea, int battleshipAmount, int cruiserAmount, int submarineAmount, int destroyerAmount, int carrierAmount)
        {
            await client.Publish(this.topic + "/GameSettings", size + "," + hitBonus + "," + restrictedArea + "," + battleshipAmount + "," + cruiserAmount + "," + submarineAmount + "," + destroyerAmount + "," + carrierAmount);
        }

        public async void SendReady()
        {
            isReady = true;
            await client.Publish(this.topic + "/Ready", "");
            CheckReadyState();
        }

        private void CheckReadyState()
        {
            Debug.WriteLine($"IsReady: {isReady}, IsEnemyReady: {isOpponentReady}");
            if (IsHost && isReady && isOpponentReady)
            {
                SendStartGame();
                OnStartGame?.Invoke();
            }
        }

        public async void SendStartGame()
        {
            await client.Publish(this.topic + "/StartGame", "");
        }

        public async void SendShoot(int x, int y)
        {
            Debug.WriteLine($"Shot at {x}, {y}");
            await client.Publish(this.topic + "/Shoot", x + "," + y);
        }

        public async void SendHit(int x, int y)
        {
            Debug.WriteLine($"Hit at {x}, {y}");
            await client.Publish(this.topic + "/Hit", x + "," + y);
        }

        public async void SendMiss(int x, int y)
        {
            Debug.WriteLine($"Miss at {x}, {y}");
            await client.Publish(this.topic + "/Miss", x + "," + y);
        }

        public async void SendSunk(int x, int y)
        {
            Debug.WriteLine($"Sunk at {x}, {y}");
            await client.Publish(this.topic + "/Sunk", x + "," + y);
        }

        public async void SendEndGame(string winner)
        {
            Debug.WriteLine($"EndGame: {winner}");
            await client.Publish(this.topic + "/EndGame", winner);
        }

        public async void Disconnected()
        {
            await client.Publish(this.topic + "/Disconnect", "");
            CloseConnection();
        }

        public void CloseConnection()
        {
            cts?.Cancel();
            cts?.Dispose();
            client.Disconnect();
        }
    }
}
