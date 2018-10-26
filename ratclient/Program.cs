using Interfaces;
using ratclient.Networking;
using Packets;
using System;
using System.Diagnostics;

namespace ratclient
{
    public static class Program
    {
        private static bool identified;
        private static Client client;

        public static void Main(string[] args)
        {
            // init
            client = new Client();

            // subscribe to events
            client.MessageReceived += OnMessageReceived;
            client.StateChanged += OnStateChanged;

            // connect
            client.Connect("localhost", 44025);

            Process.GetCurrentProcess().WaitForExit();
        }

        private static bool ProcessCommand(IPacket packet)
        {
            /*
             *  This is where you handle incoming commands
             * 
             */

            if (packet.GetType() == typeof(DoShutdownPacket))
            {
                Environment.Exit(0);
                return true;
            } else if (packet.GetType() == typeof(StringMessagePacket ))
            {
                StringMessagePacket message = (StringMessagePacket)packet;
                Console.WriteLine("Server said : " + message.Message );
                return true;
            }

            return false;
        }

        private static void OnMessageReceived(object sender, IPacket message)
        {
            /*
             * 
             *  This is where you can change how we handle incoming messages from the server
             * 
             */

            if (!ProcessCommand(message))
            {
                Console.WriteLine("Received unprocessed message: " + message);
            }
        }

        private static void OnStateChanged(object sender, bool connected)
        {
            Client node = (Client)sender;
            if (connected)
            {
                Console.WriteLine("Connected");

                if (!identified)
                    Identify(node);

                node.Send(StringMessagePacket.Create("Hello Mr. Server!!!!!!!"));
            }
            else
            {
                Console.WriteLine("Disconnected");
            }
        }

        private static void Identify(Client client)
        {
            client.Send(new IdentificationPacket()
            {
                Name = "Charlie",
                OperatingSystem = "Windows 10 Professional"
            });

            identified = true;
        }
    }
}
