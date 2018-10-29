using ratserver.Interfaces;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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

    public class BindChanger : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            string modifiedTypeName = GetModifiedTypeName(typeName);
            return Type.GetType(string.Format("{0}, {1}", modifiedTypeName, Assembly.GetEntryAssembly().FullName));
        }

        private string GetModifiedTypeName(string originalTypeName)
        {
            string oldRootNamespace = GetRootNamespace(originalTypeName);
            string currentRootNamespace = GetRootNamespace();
            return originalTypeName.Replace(oldRootNamespace, currentRootNamespace);
        }

        private string GetRootNamespace()
        {
            string assemblyName = Assembly.GetEntryAssembly().FullName;
            return assemblyName.Substring(0, assemblyName.IndexOf(','));
        }

        private string GetRootNamespace(string originalTypeName)
        {
            return originalTypeName.Substring(0, originalTypeName.IndexOf('.'));
        }
    }
}
