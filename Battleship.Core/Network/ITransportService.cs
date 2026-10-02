namespace Battleship.Core.Network
{
    public interface ITransportService
    {
        public Task Connect(Action callback);
        public Task Send(PacketCategory category, string data);
        public Task Disconnect();
    }
}
