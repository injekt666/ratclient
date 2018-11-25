
using ratserver.Interfaces;
using System;

namespace ratserver.Packets
{
    [Serializable]
    public class IdentificationPacket : IPacket
    {
        public string Name;
        public string OperatingSystem;
        public string MachineName;
        public string RAM;
        public string Version;

        public static IdentificationPacket CreateRequest()
        {
            return new IdentificationPacket();
        }
    }
}
