using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Windows;
using FragLabs.Audio.Codecs;
using System.Windows.Threading;

namespace Client
{
    class VoiceClient
    {
        public TcpClient Client = new TcpClient();
        public User User = new User();
        public event Action<string, int> SomeUserConnected;
        public event Action<string, int> SomeUserDisconnected;
        public event Action<string, int> ChannelCreated;

        public void Transmit()
        {
            var mic = Microphone.Default;
            if (mic != null)
            {
                mic.Start();
                var buffer = new byte[3840]; // mic.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(100))
                while (true)
                {
                    FrameworkDispatcher.Update();
                    for (var bytesRead = 0; bytesRead < 3528; ) // buffer.Length
                        bytesRead += mic.GetData(buffer, bytesRead, buffer.Length - bytesRead);
                    if (GetAsyncKeyState(0x11) == 0 || !Client.Connected) continue;
                    // WriteMessage(new { Type = "Voice", Data = G711Audio.ALawEncoder.ALawEncode(buffer) }); original
                    OpusEncoder encoder = OpusEncoder.Create(48000, 1, FragLabs.Audio.Codecs.Opus.Application.Voip);
                    int encodedLength;
                    byte[] encodedBuffer = encoder.Encode(buffer, buffer.Length, out encodedLength);
                    byte[] trimmedBuffer = new byte[encodedLength];
                    Array.Copy(encodedBuffer, trimmedBuffer, encodedLength);

                    WriteMessage(new { Type = "Voice", Data = trimmedBuffer });
                }
            }
            else { MessageBox.Show("Could not detect default mic"); }
        }

        private object WriteLock = new object();
        public void WriteMessage(object o)
        {
            byte[] message = UTF8Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(o));
            lock(WriteLock)
            {
                Client.GetStream().Write(BitConverter.GetBytes((short)message.Length), 0, 2); // first two bytes
                Client.GetStream().Write(message, 0, message.Length); // string of json
                Client.GetStream().Flush();
            }
        }

        public IEnumerable<dynamic> ReadMessages()
        {
            byte[] incomingMesssage = new byte[1<<14];
            int offset = 0;

            for (int numberOfBytesRead = 0; ; )
            {
                try { numberOfBytesRead = Client.GetStream().Read(incomingMesssage, offset, incomingMesssage.Length - offset); }
                catch { }

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
                            //byte[] decodedByte;
                            //G711Audio.ALawDecoder.ALawDecode(Convert.FromBase64String(message.Data.Value as string), out decodedByte);
                            OpusDecoder decoder = OpusDecoder.Create(48000, 1);
                            byte[] encodedBuffer = Convert.FromBase64String(message.Data.Value as string);
                            int decodedLength;
                            byte[] decodedBuffer = decoder.Decode(encodedBuffer, encodedBuffer.Length, out decodedLength);
                            byte[] trimmedBuffer = new byte[decodedLength];
                            Array.Copy(decodedBuffer, trimmedBuffer, decodedLength);
                            HandleVoiceMessage(trimmedBuffer);
                            break;
                        case "SomeUserConnected":
                            SomeUserConnected(message.Username.Value as string, (int)message.Channel.Value);
                            break;
                        case "SomeUserDisconnected":
                            SomeUserDisconnected(message.Username.Value as string, (int)message.Channel.Value);
                            break;
                        case "ChannelCreated":
                            ChannelCreated(message.ChannelName.Value as string, (int)message.ChannelId.Value );
                            break;
                    }
                }
            }
            catch { }
        }

        private void HandleVoiceMessage(byte[] buff)
        {
            FrameworkDispatcher.Update();
            var sound = new SoundEffect(buff, Microphone.Default.SampleRate, AudioChannels.Mono);
            sound.Play();
        }

        private void HandleChatMessage(dynamic message)
        {
        }

        public void SendChatMessage(string chatMessage)
        {
            WriteMessage(new { Type = "Chat", Username = this.User.Username, Message = chatMessage });
        }

        public void SendCredentials()
        {
            WriteMessage(new { Type="Credentials", Username=this.User.Username, Password=this.User.Password });
            WriteMessage(new { Type="Hello" });
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

                ThreadStart ts = new ThreadStart(StartReading);
                Thread thread = new Thread(ts);
                thread.IsBackground = true;
                thread.Start();

                SendCredentials();
                new Thread(Transmit) { IsBackground = true }.Start();
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

