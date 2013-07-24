using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Threading;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient client = new TcpClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void NewConnection_Click(object sender, RoutedEventArgs e)
        {
            // open new window for input information
            NewConnectionWindow ncw = new NewConnectionWindow();
            if (ncw.ShowDialog() == true) 
            {
                IPEndPoint endpoint = null;

                try
                {
                    Disconnect_Click(sender, e);
                    endpoint = ncw.IPEndPoint;
                    AddActivity("Attempting to connect to " + endpoint.Address + ":" + endpoint.Port + ".");
                    
                    client = new TcpClient();

                    //client.Connect(endpoint);

                    IAsyncResult ar = client.BeginConnect(endpoint.Address, endpoint.Port, null, null);
                    using (WaitHandle wh = ar.AsyncWaitHandle)
                    {
                        if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2), false)) throw new TimeoutException();
                        client.EndConnect(ar);
                    }

                    // play some cool robotic connected/success sound
                    AddActivity("Connected");
                    ThreadStart ts = new ThreadStart(GetStream);
                    Thread thread = new Thread(ts);
                    thread.Start();
                    Transmit();
                }
                catch(Exception ex)
                {
                    // play some cool robotic error sound
                    MessageBox.Show("Could not connect to host.");
                    AddActivity("Unable to connect to " + endpoint.Address + ":" + endpoint.Port + ".");
                    AddActivity(ex.Message + ": " + ex.StackTrace);
                }
            }
            //ncw.Show();
        }

        public void AddActivity(string s)
        {
            ActivityTextBox.Text += "\n" + s;
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (client.Connected)
            {
                client.GetStream().Close();
                client.Close();
                AddActivity("Disconnected");
            }
        }

        public void Transmit()
        {
            var wie = new WaveInEvent();

            wie.DataAvailable += (sender, e) =>
            {
                if (GetAsyncKeyState(0x11) == 0 || !client.Connected) return;

                MemoryStream buff = new MemoryStream();

                WaveFileWriter writer = new WaveFileWriter(buff, wie.WaveFormat);
                writer.Write(e.Buffer, 0, e.BytesRecorded);
                writer.Close();

                byte[] b = buff.GetBuffer();
                client.GetStream().Write(b, 0, b.Length);
                client.GetStream().Flush();
            };

            wie.StartRecording();
        }

        public void GetStream()
        {
            byte[] buf = new byte[1646];
            try
            {
                for (int i; (i = client.GetStream().Read(buf, 0, buf.Length)) > 0; )
                {
                    WaveFileReader wfr = new WaveFileReader(new MemoryStream(buf));
                    WaveOutEvent wo = new WaveOutEvent();
                    wo.Init(wfr);
                    wo.Play();
                    wo.PlaybackStopped += (sender, e) =>
                    {
                        wo.Dispose();
                        wfr.Close();
                    };
                }
            }
            catch {}
        }

        [DllImport("User32.dll")]
        static extern short GetAsyncKeyState(int vKey);
    }
}
