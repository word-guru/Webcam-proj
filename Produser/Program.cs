using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Configuration;
using AForge.Video.DirectShow;
using AForge.Video;
using System.Drawing;
using System.Drawing.Imaging;

namespace Produser
{
    internal class Program
    {
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindows();

        [DllImport("user32.dll")]

        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static IPEndPoint consumerEndPoint;
        private static UdpClient udpClient = new UdpClient();

        static void Main()
        {
            var consumerIp = ConfigurationManager.AppSettings.Get("consumerIp");
            var consumerPort = int.Parse(ConfigurationManager.AppSettings.Get("consumerPort"));
            consumerEndPoint = new IPEndPoint(IPAddress.Parse(consumerIp), consumerPort);
            Console.WriteLine($"consumer: {consumerEndPoint}");
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
            videoSource.Start();
            Console.WriteLine("\nPress Enter to hide the console...");
            Console.ReadLine();
            ShowWindow(GetConsoleWindows(), SW_HIDE);
        }

        private static void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var bmp = new Bitmap(eventArgs.Frame, 800, 600);
            try
            {
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    var bytes = ms.ToArray();
                    udpClient.Send(bytes, bytes.Length, consumerEndPoint);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
