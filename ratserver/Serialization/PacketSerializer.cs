using Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ratserver.Serialization
{
    public class PacketSerializer
    {
        private BinaryFormatter formatter;

        public PacketSerializer() : base()
        {
            formatter = new BinaryFormatter();
        }

        public void Serialize(Stream stream, IPacket packet)
        {
            formatter.Serialize(stream, packet);
        }

        public IPacket Deserialize(Stream stream)
        {
            formatter.Binder = new BindChanger();
            return (IPacket)formatter.Deserialize(stream);
        }
    }

    public class BindChanger : System.Runtime.Serialization.SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            // Define the new type to bind to
            Type typeToDeserialize = null;

            // Get the current assembly
            string currentAssembly = Assembly.GetExecutingAssembly().FullName;

            // Create the new type and return it
            typeToDeserialize = Type.GetType(string.Format("{0}, {1}", typeName, currentAssembly));

            return typeToDeserialize;
        }
    }
}
