using Battelship;
using Battelship.Lobby;
using Battleship.Lobby;
using Battleship.Logic.BattelStrategy.Modes;
using Battleship.Logic.Security;
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

namespace Battleship.Logic.Network.Online
{
    public class MQTTService
    {
        public delegate void DataReceivedDelegate(PacketCategory category, string data);
        public event DataReceivedDelegate? OnDataReceived;

        private readonly string address = "broker-cn.emqx.io";
        private readonly int port = 1883;

        private readonly MqttClient mqttClient;

        private readonly string username;
        private readonly string gameId;
        private readonly bool isHost;
        private readonly string key;

        /// <summary>
        /// Creates an object of the mqtt service
        /// </summary>
        /// <param name="gameId">The game id of the game</param>
        /// <param name="key">The key or passwort to encrypt data</param>
        /// <param name="username">The players username</param>
        /// <param name="isHost">An indicator to determine whether the player is the host of the game</param>
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

        /// <summary>
        /// Handle Debug messages from mqtt client
        /// </summary>
        /// <param name="log"></param>
        private void HandleDebug(string log)
        {
            Debug.WriteLine("Debug: " + log);
        }

        /// <summary>
        /// Connects to the mqtt broker and subscribes either to the client or host topic
        /// </summary>
        public async Task Connect()
        {
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

        /// <summary>
        /// Send a message to the mqtt broker
        /// </summary>
        /// <param name="category">The category of the data</param>
        /// <param name="data">The data to send to a specific topic</param>
        public async Task Send(PacketCategory category, string data)
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

        /// <summary>
        /// Handle received messages from mqtt broker
        /// </summary>
        /// <param name="topic">The topic in which the message is received</param>
        /// <param name="cipherText">The cipher data</param>
        /// <param name="qos">The quality of service the message is received</param>
        /// <param name="retain">An indicator to determine whether a message was retained</param>
        private void OnReceive(string topic, string cipherText, QualityOfService qos, bool retain)
        {
            string category = topic.Split('/').Last();
            Debug.WriteLine($"Data received from category {category}");
            PacketCategory topicCategory = PacketCategory.Get(category);
            string? data = AdvancedEncryptionStandard.Decrypt(cipherText, key);
            Debug.WriteLine($" <<< DataEncryption success: {data == null}");
            if (data == null)
                return;
            string sender = isHost ? "client" : "host";
            Debug.WriteLine($" >>> Receive: {sender} >>> {category} >>> {data}");
            OnDataReceived?.Invoke(topicCategory, data);
        }

        /// <summary>
        /// Disconnects from mqtt broker
        /// </summary>
        public async Task Disconnect()
        {
            await mqttClient.Disconnect();
        }
    }
}
