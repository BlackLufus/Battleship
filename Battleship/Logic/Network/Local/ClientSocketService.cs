using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Battleship.Logic.Network.Local
{
    public class ClientSocketService(string ipAddress, int port) : SocketService(IPAddress.Parse(ipAddress), port)
    {
        public event MessageHandler? OnMessageReceived;
        public event DisconnectHandler? OnDisconnect;

        public void Connect()
        {
            if (!socket.Connected)
            {
                new Thread(
                    async () =>
                    {
                        Debug.WriteLine("Client: Connecting to server");
                        await socket.ConnectAsync(endPoint);
                        Debug.WriteLine("Client: Connection accepted");
                        while (true)
                        {
                            //Debug.WriteLine("Client: Checking for messages");
                            if (IsConnected())
                            {
                                if (socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0)
                                {
                                    Disconnect();
                                    break;
                                }
                                else if (socket.Available > 0)
                                {
                                    Receive();
                                }
                            }
                            Thread.Sleep(250);
                        }
                    }).Start();
            }
        }

        public override bool IsConnected()
        {
            return socket.Connected;
        }

        protected override void Receive()
        {
            if (IsConnected())
            {
                byte[] buffer = new byte[1024];
                socket.Receive(buffer);
                SocketServiceMessage ssm = SocketServiceMessage.DecodeMessage(buffer);
                Debug.WriteLine("Client: Received message: " + ssm.Message);
                OnMessageReceived?.Invoke(ssm);
            }
        }

        public override void Send(SocketServiceMessage ssm)
        {
            if (IsConnected())
            {
                byte[] message = ssm.EncodeMessage();
                Debug.WriteLine("Client: Sending message: " + ssm.Message);
                socket.Send(message);
            }
        }

        public override void Disconnect()
        {
            if (socket.Connected)
            {
                Debug.WriteLine("Client: Disconnecting from server");
                socket.Disconnect(false);
                socket.Close();
                OnDisconnect?.Invoke();
            }
        }
    }
}
