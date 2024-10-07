using System.Net;
using System.Net.Sockets;

namespace Battleship.Network
{
    public abstract class SocketService(IPAddress ipAddress, int port)
    {
        public delegate void MessageHandler(SocketServiceMessage socketServiceMessage);
        public delegate void DisconnectHandler();

        protected IPEndPoint endPoint = new(ipAddress, port);
        protected Socket socket = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        public abstract bool IsConnected();

        public abstract void Send(SocketServiceMessage ssm);
        protected abstract void Receive();

        public abstract void Disconnect();
    }
}
