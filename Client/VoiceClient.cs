using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows;
using FragLabs.Audio.Codecs;
using System.Windows.Threading;
using Babble.Core;

namespace Client
{
    class VoiceClient
    {
        public IntPtr Owner { get; set; }
        private NetworkClient NetworkClient;
        public UserInfo User = new UserInfo();
        public event Action<string, int> SomeUserConnected;
        public event Action<string, int> SomeUserDisconnected;
        public event Action<string, int> ChannelCreated;
        public event Action<bool> Connected;
        public event Action Disconnected;
        public ISoundEngine SoundEngine = new NAudioSoundEngine();

        public void Transmit()
        {
            SoundEngine.Init();
            //Sound.Record(Owner, b =>
            //{
            //    if (GetAsyncKeyState(0x11) == 0 || NetworkClient.IsDisconnected) return;
            //    WriteMessage(Message.CreateVoiceMessage(Convert.ToBase64String(b)));
            //});
            SoundEngine.Record((b) =>
            {
                if (GetAsyncKeyState(0x11) == 0 || NetworkClient.IsDisconnected) return;
                WriteMessage(Message.CreateVoiceMessage(Convert.ToBase64String(b)));
            });
        }

        private object WriteLock = new object();
        public void WriteMessage(Message message)
        {
            lock(WriteLock)
            {
                NetworkClient.WriteMessage(message);
            }
        }

        public Message ReadMessage()
        {
            return NetworkClient.ReadMessage();
        }

        public void StartReading()
        {
            while (!NetworkClient.IsDisconnected)
            {
                var message = ReadMessage();
                if (message == null)
                {
                    return;
                }

                switch (message.Type)
                {
                    case MessageType.Voice:
                        HandleVoiceMessage(Convert.FromBase64String(message.Data as string));
                        break;
                    case MessageType.UserConnected:
                        SomeUserConnected(message.Data.Username.Value as string, (int)message.Data.ChannelId.Value);
                        break;
                    case MessageType.UserDisconnected:
                        SomeUserDisconnected(message.Data.Username.Value as string, (int)message.Data.ChannelId.Value);
                        break;
                    case MessageType.ChannelCreated:
                        ChannelCreated(message.Data.Name.Value as string, (int)message.Data.Id.Value);
                        break;
                }
            }
        }

        private void HandleVoiceMessage(byte[] buff)
        {
            //Sound.Play(Owner, buff);
            SoundEngine.Play(buff);
        }

        public void SendChatMessage(string chatMessage)
        {
            //WriteMessage(new { Type = "Chat", Username = this.User.Username, Message = chatMessage });
        }

        public void SendCredentials()
        {
            WriteMessage(Message.CreateCredentialsMessage(new UserInfo { Username = this.User.Username, Password = this.User.Password }));
            WriteMessage(Message.CreateHelloMessage());
        }

        public void Connect(string host, int port)
        {
            try
            {
                var tcpClient = new TcpClient(host, port);
                if (NetworkClient != null)
                {
                    Disconnect();
                }
                NetworkClient = new NetworkClient(tcpClient);

                ThreadStart ts = new ThreadStart(StartReading);
                Thread thread = new Thread(ts);
                thread.IsBackground = true;
                thread.Start();

                SendCredentials();
                new Thread(Transmit) { IsBackground = true }.Start();
                Connected(true);
            }
            catch { Connected(false); }
            
        }

        public void Disconnect()
        {
            if (NetworkClient == null)
            {
                return;
            }

            NetworkClient.WriteMessage(Message.CreateUserDisconnectedMessage(User));

            NetworkClient.Disconnect();
            NetworkClient = null;
        }

        [DllImport("User32.dll")]
        static extern short GetAsyncKeyState(int vKey);
    }
}

