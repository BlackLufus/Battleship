using System.Text;

namespace Battleship.Logic.Network.Local
{
    public class SocketServiceMessage(byte type, string message)
    {
        protected byte type = type;
        protected string message = message;

        public string Message
        {
            get { return message; }
        }

        public static SocketServiceMessage DecodeMessage(byte[] message)
        {
            byte messageType = message[0];
            string messageContent = Encoding.UTF8.GetString(message, 1, message.Length - 1);
            return new SocketServiceMessage(messageType, messageContent);
        }

        public byte[] EncodeMessage()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] result = new byte[messageBytes.Length + 1];
            result[0] = type;
            messageBytes.CopyTo(result, 1);
            return result;
        }
    }
}
