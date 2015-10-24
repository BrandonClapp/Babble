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
        private NetworkClient Client;
        public UserInfo User = new UserInfo();
        public event Action<string, int> SomeUserConnected;
        public event Action<string, int> SomeUserDisconnected;
        public event Action<string, int> ChannelCreated;
        public event Action<List<Channel>> RefreshChannels;
        public event Action<bool, string> Connected;
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
                if (GetAsyncKeyState(0x11) == 0 || Client.IsDisconnected) return;
                WriteMessage(new Message(MessageType.Voice, Convert.ToBase64String(b)));
            });
        }

        private object WriteLock = new object();
        public void WriteMessage(Message message)
        {
            lock(WriteLock)
            {
                Client.WriteMessage(message);
            }
        }

        public Message ReadMessage()
        {
            return Client.ReadMessage();
        }

        public void StartReading()
        {
            while (!Client.IsDisconnected)
            {
                var message = ReadMessage();
                if (message == null)
                {
                    break;
                }

                switch (message.Type)
                {
                    case MessageType.Voice:
                        HandleVoiceMessage(Convert.FromBase64String(message.Data as string));
                        break;
                    case MessageType.UserConnected:
                        var userInfo = message.GetData<UserInfo>();
                        SomeUserConnected(userInfo.Username, userInfo.ChannelId);
                        break;
                    case MessageType.UserDisconnected:
                        var userInfo2 = message.GetData<UserInfo>();
                        SomeUserDisconnected(userInfo2.Username, userInfo2.ChannelId);
                        break;
                    case MessageType.ChannelCreated:
                        var channel = message.GetData<Channel>();
                        ChannelCreated(channel.Name, channel.Id);
                        break;
                    case MessageType.RequestChannels:
                        var channels = message.GetData<List<Channel>>();
                        RefreshChannels(channels);
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

        public void Connect(string host, int port, string username, string password)

        {
            if (Client != null)
            {
                Disconnect();
                Client = null;
            }

            Client = NetworkClient.Connect(host, port);
            Client.WriteMessage(new Message(MessageType.Credential, new UserCredential() { Username = username, Password = password }));
            var credentialResult = Client.ReadMessage().GetData<UserCredentialResult>();
            if (credentialResult.IsAuthenticated)
            {
                ThreadStart ts = new ThreadStart(StartReading);
                Thread thread = new Thread(ts);
                thread.IsBackground = true;
                thread.Start();
                new Thread(Transmit) { IsBackground = true }.Start();

                Connected(true, credentialResult.Message);
                Client.WriteMessage(new Message(MessageType.RequestChannels));
            }
            else
            {
                Connected(false, credentialResult.Message);
                Client.Disconnect();
            }
        }

        public void Disconnect()
        {
            if (Client == null)
            {
                return;
            }

            Client.WriteMessage(new Message(MessageType.UserDisconnected, User));

            Client.Disconnect();
            Client = null;

            Disconnected();
        }

        [DllImport("User32.dll")]
        static extern short GetAsyncKeyState(int vKey);
    }
}

