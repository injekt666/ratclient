using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace ratserver.Networking
{
    public sealed class ClientNode : MarshalByRefObject, IDisposable
    {
        public event EventHandler<string> MessageReceived;
        public event EventHandler<bool> StateChanged;

        public bool Connected
        {
            get { return connected; }
            private set { connected = value; }
        }

        private string identifier;
        private bool connected;
        private byte[] buffer;
        private Socket socket;

        public ClientNode() : base()
        {
            buffer = new byte[1024];
        }

        public void InitializeFromSocket(Socket socket)
        {
            this.socket = socket;
            OnStateChanged(this.socket.Connected);
        }

        public void Connect(string host, int port)
        {
            BeginConnect(host, port);
        }

        private void BeginConnect(string host, int port)
        {
            if (Connected)
                return;

            if (socket == null)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            }

            socket.BeginConnect(host, port, EndConnect, this);
        }

        private void EndConnect(IAsyncResult ar)
        {
            ClientNode client = (ClientNode)ar.AsyncState;
            try
            {
                client.EndConnect(ar);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            if (client.socket.Connected)
            {
                client.OnStateChanged(true);
            }
        }

        public string GetClientIdentifier()
        {
            if (identifier == null)
            {
                identifier = socket.RemoteEndPoint.ToString();
            }

            return identifier;
        }

        public void Send(string message)
        {
            if (!Connected)
                return;

            byte[] array = Encoding.UTF8.GetBytes(message);
            Array.Resize(ref array, 1024);
            socket.BeginSend(array, 0, array.Length, SocketFlags.None, EndSend, this);
        }

        private void EndSend(IAsyncResult ar)
        {
            ClientNode client = (ClientNode)ar.AsyncState;
            try
            {
                client.socket.EndSend(ar);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void BeginReceive()
        {
            if (!Connected)
                return;

            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, EndReceive, this);
        }

        private void EndReceive(IAsyncResult ar)
        {
            ClientNode client = (ClientNode)ar.AsyncState;
            int received = 0;

            try
            {
                received = client.socket.EndReceive(ar);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                // 
                client.OnStateChanged(false);
            }

            if (received > 0)
            {
                client.ParseIncomingMessage();
            }

            client.BeginReceive();
        }

        private void ParseIncomingMessage()
        {
            string message = Encoding.UTF8.GetString(buffer).Trim('\0');
            OnMessageReceived(message);
        }

        private void OnMessageReceived(string message)
        {
            EventHandler<string> handler = MessageReceived;
            if (handler != null)
            {
                handler(this, message);
            }
        }

        private void OnStateChanged(bool connected)
        {
            if (connected)
            {
                Connected = true;
                BeginReceive();
            }
            else
            {
                Connected = false;
                socket.Close();
                socket = null;
            }

            EventHandler<bool> handler = StateChanged;
            if (handler != null)
            {
                handler(this, connected);
            }
        }

        public void Dispose()
        {
            if (socket != null)
            {
                if (Connected)
                {
                    socket.Disconnect(false);
                }

                socket.Close();
                socket.Dispose();
                socket = null;
            }
        }
    }
}
