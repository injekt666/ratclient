using ratserver.UI.Forms;
using System;
using System.Windows.Forms;

namespace ratserver
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClientManager());
        }
    }
}
