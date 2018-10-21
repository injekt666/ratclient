using ratserver.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ratserver
{
    class Program
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

        private static void OnMessageReceived(object sender, string message)
        {
            ProcessMessage((ClientNode)sender, message);
        }

        private static void ProcessMessage(ClientNode sender, string message)
        {
            /*
             *  This is where you handle incoming client messages
             * 
             */

            if (message.StartsWith("os:"))
            {
                // todo: parse operating system, and hardware id 
                //       and whatever
                sender.Send("Thank you for identifying with us, " + sender.GetClientIdentifier());
            }

            Console.WriteLine(sender.GetClientIdentifier() + " said: " + message);
        }

        private static void OnClientStateChanged(object sender, bool connected)
        {
            if (connected)
            {
                clients.Add((ClientNode)sender);
                Console.WriteLine("Client connected: " + ((ClientNode)sender).GetClientIdentifier());
            }
            else
            {
                clients.Remove((ClientNode)sender);
                Console.WriteLine("Client disconnected: " + ((ClientNode)sender).GetClientIdentifier());
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
