using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ratclient.Networking
{
    public sealed class Client : IDisposable
    {
        public event EventHandler<string> MessageReceived;
        public event EventHandler<bool> StateChanged;

        public bool Connected
        {
            get { return connected; }
            private set { connected = value; }
        }

        private bool connected;
        private Socket socket;
        private byte[] buffer;
        private string identifier;

        private const int bufferSize = 4;

        public Client() : base()
        {
            buffer = new byte[bufferSize];
        }
        
        public void InitializeFromSocket(Socket sock)
        {
            socket = sock;
            OnStateChanged(socket.Connected);
        }

        public string GetClientIdentifier()
        {
            if (identifier == null)
            {
                identifier = socket.RemoteEndPoint.ToString();
            }

            return identifier;
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
            Client client = (Client)ar.AsyncState;
            try
            {
                client.socket.EndConnect(ar);
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

        public void Send(string message)
        {
            if (!Connected)
                return;

            byte[] array = Encoding.UTF8.GetBytes(message);
            byte[] packet = new byte[4 + array.Length];
            Buffer.BlockCopy(array, 0, packet, 4, array.Length);
            packet[0] = (byte)(array.Length);
            packet[1] = (byte)(array.Length >> 8);
            packet[2] = (byte)(array.Length >> 16);
            packet[3] = (byte)(array.Length >> 24);
            socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, EndSend, this);
        }

        private void EndSend(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;
            try
            {
                client.socket.EndSend(ar);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                client.OnStateChanged(false);
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
            Client client = (Client)ar.AsyncState;
            int bytesReceived = 0;

            try
            {
                bytesReceived = client.socket.EndReceive(ar);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                // 
                client.OnStateChanged(false);
            }

            if (bytesReceived == 4)
            {
                int length = client.buffer[0] | client.buffer[1] << 8 | client.buffer[2] << 16 | client.buffer[3] << 24;
                byte[] messageBuffer = new byte[length];

                bytesReceived = client.socket.Receive(messageBuffer, 0, messageBuffer.Length, SocketFlags.None);
                if (bytesReceived == messageBuffer.Length)
                {
                    client.ParseIncomingMessage(messageBuffer );
                } // else error

            }

            client.BeginReceive();
        }

        private void ParseIncomingMessage(byte[] payload)
        {
            string message = Encoding.UTF8.GetString(payload).Trim('\0');
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

                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }
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
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Disconnect(false);
                }

                socket.Close();
                socket.Dispose();
                socket = null;
            }

            buffer = null;
            identifier = null;
        }
    }
}
