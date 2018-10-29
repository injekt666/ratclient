
using ratserver.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ratserver.Handlers;
using ratserver.Packets;
using ratserver.Interfaces;

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
            /*
           * 
           *  This is where you can change how we handle incoming messages from the clients
           * 
           */

            PacketHandler.HandlePacket((ClientNode)sender, packet);
        }

        private static void OnClientStateChanged(object sender, bool connected)
        {
            ClientNode client = (ClientNode)sender;
            if (connected)
            {
                clients.Add(client);
                Console.WriteLine("Client connected: " + client.GetClientIdentifier());

                client.Send(IdentificationPacket.CreateRequest());
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
