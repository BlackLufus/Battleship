using Mqtt.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Logic.Network
{
    public class TCPService
    {
        public delegate void ConnectionEstablishDelegate();
        public event ConnectionEstablishDelegate? OnConnectionEstablish;

        public delegate void DisconnectDelegate();
        public event DisconnectDelegate? OnDisconnect;

        public delegate void DataReceivedDelegate(PacketCategory category, string data);
        public event DataReceivedDelegate? OnDataReceived;

        protected readonly IPEndPoint endPoint;
        private TcpClient? tcpClient;
        private readonly TcpListener? tcpListener;
        private NetworkStream? stream;

        private readonly bool isHost;

        private CancellationTokenSource? cts;

        /// <summary>
        /// Creates an object of the tcp service
        /// </summary>
        /// <param name="ipAddress">The ip address to connect</param>
        /// <param name="port">The port to connect</param>
        /// <param name="isHost">An indicator to determine whether the player is the host of the game</param>
        public TCPService(IPAddress ipAddress, int port, bool isHost)
        {
            endPoint = new(isHost ? IPAddress.Any : ipAddress, port);
            if (isHost)
                tcpListener = new TcpListener(endPoint);
            else
                tcpClient = new TcpClient();

            this.isHost = isHost;
        }

        /// <summary>
        /// Connects to the tcp socket
        /// </summary>
        public async Task Connect()
        {
            // Create cancellation token source
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            // Check for connection and listen for incoming data
            await Task.Run(async () =>
            {
                // Wait for or connect a socket
                Debug.WriteLine("Host: Waiting for connection");
                if (isHost)
                {
                    try
                    {
                        tcpListener!.Start();
                        tcpClient = await tcpListener.AcceptTcpClientAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Network error: {ex.Message}");
                        return;
                    }
                }
                else
                {
                    while (true)
                    {
                        try
                        {

                            await tcpClient!.ConnectAsync(endPoint);
                            OnConnectionEstablish?.Invoke();
                            break;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Network error: {ex.Message}");
                        }
                    }
                }
                Debug.WriteLine("Host: Connection accepted");

                // Set stream
                stream = tcpClient.GetStream();

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var buffer = new byte[1024];
                        int received = await stream.ReadAsync(buffer);

                        if (received == 0)
                        {
                            Debug.WriteLine("Remote disconnected");
                            OnDisconnect?.Invoke();
                            return;
                        }

                        OnReceive(buffer);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Network error: {ex.Message}");
                        OnDisconnect?.Invoke();
                        return;
                    }
                }
            }, token);
        }

        /// <summary>
        /// Send a message to the connected socket
        /// </summary>
        /// <param name="category">The category of the data</param>
        /// <param name="data">The data to send to a specific topic</param>
        public async Task Send(PacketCategory category, string data)
        {
            if (stream == null)
                return;

            // Build message
            string message = $"{category}/{data}";

            string sender = isHost ? "client" : "host";
            Debug.WriteLine($" >>> Send: {sender} >>> {category} >>> {data}");

            // Convert message to bytes
            byte[] bytes = Encoding.UTF8.GetBytes(message);

            // Send bytes
            await stream.WriteAsync(bytes);
        }

        /// <summary>
        /// Handle received messages from socket
        /// </summary>
        /// <param name="buffer">The buffer with the encoded message
        private void OnReceive(byte[] buffer)
        {
            // Encode buffer to string
            string message = Encoding.UTF8.GetString(buffer, 0, buffer.Length - 1);

            // Get category
            string category = message.Split('/').First();
            PacketCategory topicCategory = PacketCategory.Get(category);

            // Get message
            string data = message.Split('/').Last();

            string sender = isHost ? "client" : "host";
            Debug.WriteLine($" >>> Receive: {sender} >>> {category} >>> {data}");

            OnDataReceived?.Invoke(topicCategory, data);
        }

        /// <summary>
        /// Disconnects from socket
        /// </summary>
        public void Disconnect()
        {
            Debug.WriteLine("\n >>> TCP Service: Connection close!");
            cts?.Cancel();
            
            stream?.Close();
            stream = null;

            tcpClient?.Close();
            tcpListener?.Stop();
        }
    }
}
