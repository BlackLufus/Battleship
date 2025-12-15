using Battelship;
using Battelship.Lobby;
using Battleship.Lobby;
using Battleship.Logic.BattelStrategy.Modes;
using Battleship.Logic.Global;
using Mqtt.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Battleship.Logic.Network
{
    public class TopicCategory
    {
        public string Value { get; private set; }

        private TopicCategory(string value)
        {
            Value = value;
        }

        public static readonly TopicCategory PING = new TopicCategory("Ping");
        public static readonly TopicCategory CONNECT = new TopicCategory("Connect");
        public static readonly TopicCategory CONNACK = new TopicCategory("ConnAck");
        public static readonly TopicCategory SETTINGS = new TopicCategory("Settings");
        public static readonly TopicCategory SETTINGSACK = new TopicCategory("SettingsAck");
        public static readonly TopicCategory READY = new TopicCategory("Ready");
        public static readonly TopicCategory READYACK = new TopicCategory("ReadyAck");
        public static readonly TopicCategory SHOOT = new TopicCategory("Shoot");
        public static readonly TopicCategory MISS = new TopicCategory("Miss");
        public static readonly TopicCategory HIT = new TopicCategory("Hit");
        public static readonly TopicCategory SUNK = new TopicCategory("Sunk");
        public static readonly TopicCategory MESSAGE = new TopicCategory("Message");
        public static readonly TopicCategory MESSAGEACK = new TopicCategory("MessageAck");
        public static readonly TopicCategory DISCONNECT = new TopicCategory("DISCONNECT");

        public override string ToString()
        {
            return Value;
        }

        public static TopicCategory Get(string value)
        {
            return value switch
            {
                "Ping" => PING,
                "Connect" => CONNECT,
                "ConnAck" => CONNACK,
                "Settings" => SETTINGS,
                "SettingsAck" => SETTINGSACK,
                "Ready" => READY,
                "ReadyAck" => READYACK,
                "Shoot" => SHOOT,
                "Miss" => MISS,
                "Hit" => HIT,
                "Sunk" => SUNK,
                "Message" => MESSAGE,
                "MessageAck" => MESSAGEACK,
                _ => throw new ArgumentException($"Unknown TopicCategory: {value}"),
            };
        }
    }

    public class MQTTService
    {
        public delegate void DataReceivedDelegate(TopicCategory category, string data);
        public event DataReceivedDelegate? OnDataReceived;

        private readonly string address = "broker-cn.emqx.io";
        private readonly int port = 1883;

        private readonly MqttClient mqttClient;

        private readonly string username;
        private readonly string gameId;
        private readonly bool isHost;
        private readonly string key;

        public MQTTService(string gameId, string key, string username, bool isHost)
        {
            this.username = username;
            mqttClient = new MqttClient(
                new MqttOption
                {
                    Version = MqttVersion.MQTT_3_1_1,
                    WillRetain = false,
                    CleanSession = true,
                    KeepAlive = 60
                },
                debug: HandleDebug
            );
            mqttClient.OnMessageReceived += OnReceive;

            // Set Host role
            this.isHost = isHost;

            // Set game ID
            this.gameId = gameId;

            // Set key
            this.key = key;
        }

        private void HandleDebug(string log)
        {
            Console.WriteLine("Debug: " + log);
        }

        public async Task Connect()
        {
            string sender = isHost ? "host" : "client";
            string mqttUsername = $"{username}";

            await mqttClient.Connect(address, port, mqttUsername);
            Debug.WriteLine($"Is host {isHost}");
            if (isHost)
            {
                string topic = $"battelship/{gameId}/client/#";
                Debug.WriteLine($"Subscribe to {topic}");
                await mqttClient.SubscribeAsync(topic);
            }
            if (!isHost)
            {
                string topic = $"battelship/{gameId}/host/#";
                Debug.WriteLine($"Subscribe to {topic}");
                await mqttClient.SubscribeAsync(topic);
            }
        }

        public async Task Send(TopicCategory category, string data)
        {
            Debug.WriteLine($" >>> {category} >>> {data}");
            string? cipherText = AdvancedEncryptionStandard.Encrypt(data, key);
            if (cipherText == null)
                return;

            string sender = isHost ? "host" : "client";
            string topic = $"battelship/{gameId}/{sender}/{category}";
            Debug.WriteLine($"Publish to {topic}");
            await mqttClient.PublishAsync(topic, cipherText);
        }

        private void OnReceive(string topic, string cipherText, QualityOfService qos, bool retain)
        {
            string category = topic.Split('/').Last();
            Debug.WriteLine($"Data received from category {category}");
            TopicCategory topicCategory = TopicCategory.Get(category);
            string? data = AdvancedEncryptionStandard.Decrypt(cipherText, key);
            Debug.WriteLine($" <<< DataEncryption success: {data == null}");
            if (data == null)
                return;
            string sender = isHost ? "client" : "host";
            Debug.WriteLine($" >>> Receive: {sender} >>> {category} >>> {data}");
            OnDataReceived?.Invoke(topicCategory, data);
        }
    }
}
