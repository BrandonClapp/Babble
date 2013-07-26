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
            var wie = new WaveInEvent();

            wie.DataAvailable += (sender, e) =>
            {
                if (GetAsyncKeyState(0x11) == 0 || !Client.Connected) return;

                MemoryStream buff = new MemoryStream();

                WaveFileWriter writer = new WaveFileWriter(buff, wie.WaveFormat);
                writer.Write(e.Buffer, 0, e.BytesRecorded);
                writer.Close();

                byte[] b = buff.GetBuffer();
                WriteMessage(
                new 
                { 
                    Type = "Voice", 
                    Data = b
                });
            };

            wie.StartRecording();
        }

        public void WriteMessage(object o)
        {
            byte[] message = UTF8Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(o));
            lock(WriteLock)
            {
                Client.GetStream().Write(BitConverter.GetBytes((short)message.Length), 0, 2); // first two bytes
                Client.GetStream().Write(message, 0, message.Length); // string of jason
                Client.GetStream().Flush();
            }
        }

        private object WriteLock = new object();

        public IEnumerable<dynamic> ReadMessages()
        {
            byte[] incomingMesssage = new byte[4096];
            int offset = 0;

            // Read each byte from the stream
            for (int numberOfBytesRead = 0; ; )
            {
                try
                {
                    numberOfBytesRead = Client.GetStream().Read(incomingMesssage, offset, incomingMesssage.Length - offset);
                }
                catch(Exception e) 
                {
                    var s = "";
                }

                offset += numberOfBytesRead;
                while (offset >= 2 && offset >= BitConverter.ToInt16(incomingMesssage, 0) + 2)
                {
                    short messageLength = BitConverter.ToInt16(incomingMesssage, 0);
                    yield return JsonConvert.DeserializeObject<dynamic>
                        (UTF8Encoding.UTF8.GetString(incomingMesssage, 2, messageLength));
                    offset -= (messageLength + 2);
                    Array.Copy(incomingMesssage, messageLength + 2, incomingMesssage, 0, offset);
                }
            }
        }

        public void StartReading()
        {
            try
            {
                foreach (dynamic message in ReadMessages())
                {
                    switch (message.Type.Value as string)
                    {
                        case "Voice":
                            var t = message.Data.GetType();
                            var b = message.Data.Value;
                            var tp = b.GetType();
                            var b6 = Convert.FromBase64String(message.Data.Value as string);
                            HandleVoiceMessage(b6);
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

        public void SendChatMessage(string chatMessage)
        {
            WriteMessage(new { Type = "Chat", Username = this.Username, Message = chatMessage });
        }

        public void SendCredentials()
        {
            WriteMessage(new { Type="Credentials", Username=this.Username, Password=this.Password });
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

                SendCredentials();
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

