using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Logic.Network
{
    public class PacketCategory
    {
        public string Value { get; private set; }

        private PacketCategory(string value)
        {
            Value = value;
        }

        public static readonly PacketCategory PING = new PacketCategory("Ping");
        public static readonly PacketCategory CONNECT = new PacketCategory("Connect");
        public static readonly PacketCategory CONNACK = new PacketCategory("ConnAck");
        public static readonly PacketCategory SETTINGS = new PacketCategory("Settings");
        public static readonly PacketCategory SETTINGSACK = new PacketCategory("SettingsAck");
        public static readonly PacketCategory READY = new PacketCategory("Ready");
        public static readonly PacketCategory READYACK = new PacketCategory("ReadyAck");
        public static readonly PacketCategory SHOOT = new PacketCategory("Shoot");
        public static readonly PacketCategory MISS = new PacketCategory("Miss");
        public static readonly PacketCategory HIT = new PacketCategory("Hit");
        public static readonly PacketCategory SUNK = new PacketCategory("Sunk");
        public static readonly PacketCategory MESSAGE = new PacketCategory("Message");
        public static readonly PacketCategory MESSAGEACK = new PacketCategory("MessageAck");
        public static readonly PacketCategory DISCONNECT = new PacketCategory("DISCONNECT");

        public override string ToString()
        {
            return Value;
        }

        public static PacketCategory Get(string value)
        {
            return value switch
            {
                "Ping" => PING,
                "Connect" => CONNECT,
                "ConnAck" => CONNACK,
                "Settings" => SETTINGS,
                "SettingsAck" => SETTINGSACK,
                "Ready" => READY,
                "ReadyAck" => READYACK,
                "Shoot" => SHOOT,
                "Miss" => MISS,
                "Hit" => HIT,
                "Sunk" => SUNK,
                "Message" => MESSAGE,
                "MessageAck" => MESSAGEACK,
                "DISCONNECT" => DISCONNECT,
                _ => throw new ArgumentException($"Unknown PacketCategory: {value}"),
            };
        }
    }
}
