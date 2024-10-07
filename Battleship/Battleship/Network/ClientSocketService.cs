using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Battleship.Network
{
    public class ClientSocketService(string ipAddress, int port) : SocketService(IPAddress.Parse(ipAddress), port)
    {
        public event MessageHandler? OnMessageReceived;
        public event DisconnectHandler? OnDisconnect;

        public void Connect()
        {
            if (!this.socket.Connected)
            {
                new Thread(
                    async () =>
                    {
                        Debug.WriteLine("Client: Connecting to server");
                        await this.socket.ConnectAsync(this.endPoint);
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
                                else if (this.socket.Available > 0)
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
            return this.socket.Connected;
        }

        protected override void Receive()
        {
            if (IsConnected())
            {
                byte[] buffer = new byte[1024];
                this.socket.Receive(buffer);
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
                this.socket.Send(message);
            }
        }

        public override void Disconnect()
        {
            if (this.socket.Connected)
            {
                Debug.WriteLine("Client: Disconnecting from server");
                this.socket.Disconnect(false);
                this.socket.Close();
                OnDisconnect?.Invoke();
            }
        }
    }
}
