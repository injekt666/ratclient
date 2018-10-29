
using ratclient.Interfaces;
using ratclient.Serialization;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ratclient.Networking
{
    public sealed class Client : IDisposable
    {
        public event EventHandler<IPacket> MessageReceived;
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
        private PacketSerializer serializer;

        private const int bufferSize = 4;

        public Client() : base()
        {
            buffer = new byte[bufferSize];
            serializer = new PacketSerializer();
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

        public void Send(IPacket packet)
        {
            if (!Connected)
                return;

            byte[] serializedPacket = null;
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, packet);
                serializedPacket = stream.ToArray();

                stream.SetLength(0L);

                stream.WriteByte((byte)serializedPacket.Length);
                stream.WriteByte((byte)(serializedPacket.Length >> 8));
                stream.WriteByte((byte)(serializedPacket.Length >> 16));
                stream.WriteByte((byte)(serializedPacket.Length >> 24));
                stream.Write(serializedPacket, 0, serializedPacket.Length);

                serializedPacket = stream.ToArray();
            }

            socket.BeginSend(serializedPacket, 0, serializedPacket.Length, SocketFlags.None, EndSend, this);
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
            //string message = Encoding.UTF8.GetString(payload).Trim('\0');
            IPacket packet = null;
            using (MemoryStream stream = new MemoryStream(payload))
            {
                 packet = serializer.Deserialize(stream);
            }
            OnMessageReceived(packet);
        }

        private void OnMessageReceived(IPacket  packet)
        {
            EventHandler<IPacket> handler = MessageReceived;
            if (handler != null)
            {
                handler(this, packet);
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
