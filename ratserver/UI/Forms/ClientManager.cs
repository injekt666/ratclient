using ratserver.Interfaces;
using ratserver.Networking;
using ratserver.Packets;
using System.ComponentModel;
using System.Windows.Forms;

namespace ratserver.UI.Forms
{
    public partial class ClientManager : Form
    {
        private ServerManager servManager;

        public ClientManager()
        {
            SetupEvents();
            InitializeComponent();
            servManager = new ServerManager();
        }

        private void SetupEvents()
        {
            RATServerInstance.Server.ClientMessageReceived += OnClientMessageReceived;
            RATServerInstance.Server.ClientStateChanged += OnClientStateChanged;
            RATServerInstance.Server.ServerStateChanged += OnServerStateChanged;
        }

        private void OnClientMessageReceived(object sender, IPacket packet)
        {
            ClientNode node = (ClientNode)sender; 
            if (packet.GetType() == typeof(IdentificationPacket))
            {
                if (listClients.InvokeRequired)
                {
                    listClients.Invoke((MethodInvoker)(() =>
                    {
                        OnClientMessageReceived(sender, packet);
                    }));

                    return;
                }

                IdentificationPacket ident = (IdentificationPacket)packet;
                foreach (ListViewItem item in listClients.Items)
                {
                    if (item.Tag == node)
                    {
                        item.Text = ident.Name;
                        item.SubItems.AddRange(new string[] { node.GetClientIdentifier(), ident.MachineName , ident.OperatingSystem , ident.RAM, ident.Version});
                        break;
                    }
                }
            }
        }

        private void OnClientStateChanged(object sender, bool connected)
        {
            if (listClients.InvokeRequired)
            {
                listClients.Invoke((MethodInvoker)(() =>
                {
                    OnClientStateChanged(sender, connected);
                }));

                return;
            }

            ClientNode node = (ClientNode)sender;
            if (connected)
            {
                ListViewItem client = new ListViewItem();
                client.Tag = node;
                listClients.Items.Add(client);

                node.Send(new IdentificationPacket());
            }
            else
            {
                foreach (ListViewItem item in listClients.Items)
                {
                    if (item.Tag == node)
                    {
                        listClients.Items.Remove(item);
                        break;
                    }
                }
            }
        }

        private void OnServerStateChanged(object sender, bool listening)
        {

        }

        private void contextClient_Opening(object sender, CancelEventArgs e)
        {
            ContextMenuStrip context = (ContextMenuStrip)sender;
            context.Enabled = listClients.SelectedItems.Count > 0;
        }

        private void btnServerSettings_Click(object sender, System.EventArgs e)
        {
            servManager.ShowDialog();
        }

        private void btnClientDisconnect_Click(object sender, System.EventArgs e)
        {
            foreach(ListViewItem item in listClients.SelectedItems )
            {
                ClientNode node = (ClientNode)item.Tag;
                node.Send(new DoShutdownPacket());
            }
        }
    }
}
