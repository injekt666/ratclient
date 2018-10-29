using Interfaces;
using Packets;
using ratserver.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ratserver.Handlers
{
    public static class PacketHandler
    {
        public static void HandlePacket(ClientNode client, IPacket packet)
        {
            Type packetType = packet.GetType();
            if (packetType == typeof(IdentificationPacket))
            {
                CommandHandler.HandleCommand(client, (IdentificationPacket)packet);
            }
            else if (packetType == typeof(StringMessagePacket))
            {
                CommandHandler.HandleCommand(client, (StringMessagePacket)packet);
            }
        }
    }
}
