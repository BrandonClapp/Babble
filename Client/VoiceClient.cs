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
        private NetworkClient Client;
        public UserInfo User = new UserInfo();
        public event Action<string, int> SomeUserConnected;
        public event Action<string, int> SomeUserDisconnected;
        public event Action<string> SomeUserTalking;
        public event Action<string, int> ChannelCreated;
        public event Action<List<Channel>> RefreshChannels;
        public event Action<bool, string> Connected;
        public event Action Disconnected;
        public ISoundEngine SoundEngine = new NAudioSoundEngine();

        public void Transmit()
        {
            SoundEngine.Init();

            SoundEngine.Record((b) =>
            {
                if (GetAsyncKeyState(0x11) == 0 || Client.IsDisconnected) return;
                var voiceData = new VoiceData();
                voiceData.Username = User.Username;
                voiceData.SetDataFromBytes(b);
                WriteMessage(Message.Create(MessageType.Voice, voiceData));
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
                        var voiceData = message.GetData<VoiceData>();
                        HandleVoiceMessage(voiceData);
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
                    case MessageType.UserChangeChannelResponse:
                        // youarehere
                        var targetChannel = message.GetData<Channel>();
                        //RefreshChannels(channel);
                        break;
                }
            }
        }

        private void HandleVoiceMessage(VoiceData voiceData)
        {
            SoundEngine.Play(voiceData.GetDataInBytes());
            SomeUserTalking(voiceData.Username);
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
            Client.WriteMessage(Message.Create(MessageType.Credential, new UserCredential() { Username = username, Password = password }));
            var credentialResult = Client.ReadMessage().GetData<UserCredentialResult>();
            if (credentialResult.IsAuthenticated)
            {
                User = credentialResult.UserInfo;

                ThreadStart ts = new ThreadStart(StartReading);
                Thread thread = new Thread(ts);
                thread.IsBackground = true;
                thread.Start();
                new Thread(Transmit) { IsBackground = true }.Start();

                Connected(true, credentialResult.Message);
                Client.WriteMessage(Message.Create(MessageType.RequestChannels));
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

            if (!Client.IsDisconnected)
            {
                Client.WriteMessage(Message.Create(MessageType.UserDisconnected, User));
            }


            Client.Disconnect();
            Client = null;

            Disconnected();
        }

        [DllImport("User32.dll")]
        static extern short GetAsyncKeyState(int vKey);
    }
}

