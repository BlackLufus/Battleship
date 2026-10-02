using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Logic.Network
{
    public interface ITransportService
    {
        public Task Connect(Action callback);
        public Task Send(PacketCategory category, string data);
        public Task Disconnect();
    }
}
