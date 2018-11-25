using ratserver.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ratserver
{
    internal static class RATServerInstance
    {
        public const string Version = "v1.0.0.0";
        public static Server Server;

        static RATServerInstance()
        {
            Server = new Server();
        }

        public static bool Listen(int port)
        {
            return Server.Listen(port);
        }

        // todo: RATServerInstance.StopListening();
    }
}
