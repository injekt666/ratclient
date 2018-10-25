using ratclient.Networking;
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

        private static bool ProcessCommand(string cmd)
        {
            /*
             *  This is where you handle incoming commands
             * 
             */

            if (cmd == "shutdown")
            {
                Environment.Exit(0);
                return true;
            }

            return false;
        }

        private static void OnMessageReceived(object sender, string message)
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
                {
                    Console.WriteLine("Sending identification");

                    Identify(node);
                    identified = true;
                }
            }
            else
            {
                Console.WriteLine("Disconnected");
            }
        }

        private static void Identify(Client client)
        {
            client.Send("os:Windows 10 Professional|hwid:" + Guid.NewGuid().ToString());
        }
    }
}
