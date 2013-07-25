using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json;
using NAudio.Wave;

namespace Client
{
    class User
    {
        public TcpClient Client = new TcpClient();
        public string Username { get; set; }
        private string passwordHash;

        public string Password
        {
            get
            {
                return this.passwordHash;
            }
            set
            {
                this.passwordHash = UTF8Encoding.UTF8.GetString(SHA1.Create().ComputeHash(UTF8Encoding.UTF8.GetBytes(value)));
            }
        }

        public void Transmit()
        {
            StreamWriter sw = new StreamWriter(Client.GetStream());
            Client.SendBufferSize = 4096;

            sw.Write(JsonConvert.SerializeObject(
                new 
                { 
                    Type = "Credentials", 
                    Username = this.Username, 
                    Password = this.Password 
                }));
            sw.Flush();

            var wie = new WaveInEvent();

            wie.DataAvailable += (sender, e) =>
            {
                if (GetAsyncKeyState(0x11) == 0 || !Client.Connected) return;

                MemoryStream buff = new MemoryStream();

                WaveFileWriter writer = new WaveFileWriter(buff, wie.WaveFormat);
                writer.Write(e.Buffer, 0, e.BytesRecorded);
                writer.Close();

                byte[] b = buff.GetBuffer();
                sw.Write(JsonConvert.SerializeObject(
                new 
                { 
                    Type = "Voice", 
                    Data = b
                }));
                sw.Flush();
            };

            wie.StartRecording();
        }

        public void StartReading()
        {
            byte[] buf = new byte[4096];
            try
            {
                for (int i; (i = Client.GetStream().Read(buf, 0, buf.Length)) > 0; )
                {
                    dynamic message = JsonConvert.DeserializeObject<dynamic>(UTF8Encoding.UTF8.GetString(buf, 0, i));
                    switch (message.Type as string)
                    {
                        case "Voice":
                            HandleVoiceMessage(message.Data as byte[]);
                            break;
                    }
                }
            }
            catch { }
        }

        private void HandleVoiceMessage(byte[] buff)
        {
            WaveFileReader wfr = new WaveFileReader(new MemoryStream(buff));
            WaveOutEvent wo = new WaveOutEvent();
            wo.Init(wfr);
            wo.Play();
            wo.PlaybackStopped += (sender, e) =>
            {
                wo.Dispose();
                wfr.Close();
            };
        }

        public bool Connect(IPEndPoint endpoint)
        {
            try
            {
                Client = new TcpClient();

                IAsyncResult ar = Client.BeginConnect(endpoint.Address, endpoint.Port, null, null);
                using (WaitHandle wh = ar.AsyncWaitHandle)
                {
                    if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2), false)) throw new TimeoutException();
                    Client.EndConnect(ar);
                }

                // play some cool robotic connected/success sound
                ThreadStart ts = new ThreadStart(StartReading);
                Thread thread = new Thread(ts);
                thread.IsBackground = true;
                thread.Start();
                Transmit();
                return true;
            }
            catch { }
            return false;
        }

        public void Disconnect()
        {
            if (Client.Connected)
            {
                Client.GetStream().Close();
                Client.Close();
            }
        }

        [DllImport("User32.dll")]
        static extern short GetAsyncKeyState(int vKey);

    }
}

