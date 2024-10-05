namespace Battleship.Services
{
    public class LobbyServiceMessage(LobbyServiceMessage.MessageType type, string message) : SocketServiceMessage((byte)type, message)
    {
        public enum MessageType
        {
            Name = 0x00,
            FieldSize = 0x01,
            HitBonus = 0x02,
            RestritedArea = 0x03,
            AmountOfShips = 0x04,
            Ready = 0x05,
            NotReady = 0x06,
            StartGame = 0x07,
            LeftGame = 0x08,
        }

        public MessageType getType()
        {
            return (MessageType)type;
        }
    }
}
