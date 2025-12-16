using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Battleship.Logic.Network.Local
{
    public class HostSocketService(string ipAddress, int port) : SocketService(IPAddress.Parse(ipAddress), port)
    {
        public event MessageHandler? OnMessageReceived;
        public event DisconnectHandler? OnDisconnect;

        private Socket? handler;

        public void Start()
        {
            socket.Bind(endPoint);
            socket.Listen(100);
            new Thread(
                async () =>
                {
                    Debug.WriteLine("Host: Waiting for connection");
                    handler = await socket.AcceptAsync();
                    Debug.WriteLine("Host: Connection accepted");
                    while (true)
                    {
                        if (!IsConnected() || handler.Poll(1000, SelectMode.SelectRead) && handler.Available == 0)
                        {
                            Disconnect();
                            break;
                        }
                        else if (handler.Available > 0)
                        {
                            Receive();
                        }
                        Thread.Sleep(250);
                    }
                }).Start();
        }

        public override bool IsConnected()
        {
            return handler != null && handler.Connected;
        }

        protected override async void Receive()
        {
            if (IsConnected())
            {
                var buffer = new byte[1024];
                var received = await handler!.ReceiveAsync(buffer, SocketFlags.None);
                SocketServiceMessage ssm = SocketServiceMessage.DecodeMessage(buffer);
                Debug.WriteLine("Host: Received message: " + ssm.Message);
                OnMessageReceived?.Invoke(ssm);
            }
        }

        public override void Send(SocketServiceMessage ssm)
        {
            if (IsConnected())
            {
                byte[] message = ssm.EncodeMessage();
                Debug.WriteLine("Host: Sending message: " + ssm.Message);
                handler!.Send(message);
            }
        }

        public override void Disconnect()
        {
            if (IsConnected())
            {
                Debug.WriteLine("Host: Disconnected");
                handler!.Shutdown(SocketShutdown.Both);
                handler.Close();
                handler = null;
                socket.Close();
                OnDisconnect?.Invoke();
            }
        }
    }
}
