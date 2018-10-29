using System;
using System.Diagnostics;
using ratclient.Networking;
using ratclient.Packets;
using ratclient.Handlers;
using ratclient.Interfaces;


namespace ratclient
{
    public static class Program
    {
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

        private static void OnMessageReceived(object sender, IPacket message)
        {
            /*
             * 
             *  This is where you can change how we handle incoming messages from the server
             * 
             */

            PacketHandler.HandlePacket((Client)sender, message);
        }

        private static void OnStateChanged(object sender, bool connected)
        {
            Client node = (Client)sender;
            if (connected)
            {
                Console.WriteLine("Connected");
            }
            else
            {
                Console.WriteLine("Disconnected");
            }
        }
        
    }
}
