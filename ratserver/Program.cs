using Interfaces;
using ratserver.Networking;
using Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ratserver
{
    public static class Program
    {
        private static Server server;
        private static List<ClientNode> clients;

        public static void Main(string[] args)
        {
            clients = new List<ClientNode>();

            server = new Server();
            server.ClientMessageReceived += OnMessageReceived;
            server.ClientStateChanged += OnClientStateChanged;
            server.ServerStateChanged += OnServerStateChanged;
            server.Listen(44025);

            Process.GetCurrentProcess().WaitForExit();
        }

        private static void OnMessageReceived(object sender, IPacket packet)
        {
            ProcessMessage((ClientNode)sender, packet);
        }

        private static void ProcessMessage(ClientNode sender, IPacket packet)
        {
            /*
             *  This is where you handle incoming client messages
             * 
             */

            if (packet.GetType() == typeof(IdentificationPacket))
            {
                IdentificationPacket identity = (IdentificationPacket)packet;
                Console.WriteLine(sender.GetClientIdentifier() + " identified themselves as " + identity.Name + ". They are currently running " + identity.OperatingSystem );
            } else if (packet.GetType() == typeof(StringMessagePacket ))
            {
                StringMessagePacket message = (StringMessagePacket)packet;
                Console.WriteLine(sender.GetClientIdentifier() + " said: " + message.Message);
            }
        }

        private static void OnClientStateChanged(object sender, bool connected)
        {
            ClientNode client = (ClientNode)sender;
            if (connected)
            {
                clients.Add(client);
                Console.WriteLine("Client connected: " + client.GetClientIdentifier());
            }
            else
            {
                clients.Remove(client);
                Console.WriteLine("Client disconnected: " + client.GetClientIdentifier());
            }
        }

        private static void OnServerStateChanged(object sender, bool listening)
        {
            if (listening)
            {
                Console.WriteLine("Server is listening");
            }
            else
            {
                Console.WriteLine("Server is closed");
            }
        }
    }
}
