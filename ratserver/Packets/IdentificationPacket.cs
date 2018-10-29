using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    [Serializable ]
    public class IdentificationPacket : IPacket
    {
        public string Name;
        public string OperatingSystem;

        public static IdentificationPacket Create(string name, string os)
        {
            return new IdentificationPacket()
            {
                Name = name,
                OperatingSystem = os
            };
        }

        public static IdentificationPacket CreateRequest()
        {
            return new IdentificationPacket();
        }
    }
}
