using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ratserver.UI.Forms
{
    public partial class ServerManager : Form
    {
        public ServerManager()
        {
            InitializeComponent();
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            RATServerInstance.Listen((int)numPort.Value);
        }
    }
}
