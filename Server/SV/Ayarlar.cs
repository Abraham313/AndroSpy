using System;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Ayarlar : Form
    {
        Socket sock; public string ID = "";
        public Ayarlar(Socket sck, string aydi)
        {
            InitializeComponent();
            sock = sck; ID = aydi;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            yenile();
        }
        public void yenile()
        {
            try
            {
                Form1.KomutGonder("VOLUMELEVELS", "[VERI][0x09]", sock);
            }
            catch (Exception) { }
        }
        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                Form1.KomutGonder("ZILSESI", "[VERI]" + trackBar1.Value.ToString() + "[VERI][0x09]", sock);
                yenile();
            }
            catch (Exception) { }
        }

        private void trackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                Form1.KomutGonder("MEDYASESI", "[VERI]" + trackBar2.Value.ToString() + "[VERI][0x09]", sock);
                yenile();
            }
            catch (Exception) { }
        }

        private void trackBar3_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                Form1.KomutGonder("BILDIRIMSESI", "[VERI]" + trackBar3.Value.ToString() + "[VERI][0x09]", sock);
                yenile();
            }
            catch (Exception) { }
        }

        private void trackBar4_MouseUp(object sender, MouseEventArgs e)
        {
            //PARLAKLIK
            try
            {
                Form1.KomutGonder("PARLAKLIK", "[VERI]" + trackBar4.Value.ToString() + "[VERI][0x09]", sock);
                yenile();
            }
            catch (Exception) { }
        }
    }
}
