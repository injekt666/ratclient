
using ratserver.Interfaces;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ratserver.Networking
{
    public sealed class Server : IDisposable
    {
        public event EventHandler<bool> ServerStateChanged;
        public event EventHandler<bool> ClientStateChanged;
        public event EventHandler<IPacket> ClientMessageReceived;

        private Socket socket;

        public bool Listen(int port)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
            socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Bind(endPoint);
                socket.Listen(100);
                OnServerStateChanged(true);
                return true;
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        
        private void BeginAccept()
        {
            socket.BeginAccept(EndAccept, this);
        }

        private void EndAccept(IAsyncResult ar )
        {
            ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false);

            Server server = (Server)ar.AsyncState;
            Socket client = server.socket.EndAccept(ar);

            ClientNode node = new ClientNode();
            node.StateChanged += OnClientStateChanged;
            node.MessageReceived += OnClientMessageReceived;
            node.InitializeFromSocket(client);

            server.BeginAccept();
        }

        private void OnClientMessageReceived(object sender, IPacket packet)
        {
            EventHandler<IPacket> handler = ClientMessageReceived;
            if (handler != null)
            {
                handler(sender, packet);
            }
        }

        private void OnClientStateChanged(object sender, bool connected)
        {
            if (connected)
            {
                 // add client to list
            }
            else
            {
                // remove client from list
            }

            EventHandler<bool> handler = ClientStateChanged;
            if (handler != null)
            {
                handler(sender, connected);
            }
        }

        private void OnServerStateChanged(bool listening)
        {
            if (listening)
            {
                BeginAccept();
            }

            EventHandler<bool> handler = ServerStateChanged;
            if (handler != null)
            {
                handler(this, listening);
            }
        }

        public void Dispose()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
