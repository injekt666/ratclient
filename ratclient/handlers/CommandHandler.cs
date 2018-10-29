using Microsoft.VisualBasic.Devices;
using Packets;
using ratclient.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ratclient.Handlers
{
    public partial class CommandHandler
    {
        public static void HandleCommand(Client client, DoShutdownPacket packet)
        {
            if (Application.MessageLoop)
            {
                Application.Exit();
            }
            else
            {
                Environment.Exit(0);
            }
        }

        public static void HandleCommand(Client client, IdentificationPacket packet)
        {
            string userName = Environment.UserName;
            string osFriendlyName = new ComputerInfo().OSFullName;
            string architecture = Environment.Is64BitOperatingSystem ? " (x64)" : " (x86)";
            osFriendlyName = osFriendlyName + architecture;

            client.Send(
                IdentificationPacket.Create(userName, osFriendlyName)
            );
        }

        public static void HandleCommand(Client client, StringMessagePacket packet)
        {
            Console.WriteLine(packet.Message);
        }
    }
}
