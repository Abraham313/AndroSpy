using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Konum : Form
    {
        Socket sck; public string ID = "";
        public Konum(Socket soket, string aydi)
        {
            InitializeComponent();
            sck = soket; ID = aydi;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] konum_al = Encoding.UTF8.GetBytes("KONUM|");
                Form1.KomutGonder("KONUM", "[VERI]",sck);
            }
            catch (Exception) { }
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try { Process.Start(e.LinkText); } catch (Exception) { }
        }
    }
}
