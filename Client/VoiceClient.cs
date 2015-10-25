using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows;
using Babble.Core;
using System.Threading.Tasks;

namespace Client
{
    class VoiceClient
    {
        private NetworkClient Client;
        public UserInfo UserInfo = new UserInfo();
        public event Action<UserInfo> SomeUserConnected;
        public event Action<UserInfo> SomeUserDisconnected;
        public event Action<UserInfo> SomeUserTalking;
        public event Action<UserInfo> SomeUserChangedChannel;
        public event Action<Channel> ChannelCreated;
        public event Action<Channel> ChannelRenamed;
        public event Action<List<Channel>> RefreshChannels;
        public event Action<bool, string> Connected;
        public event Action Disconnected;
        public ISoundEngine SoundEngine;

        public VoiceClient()
        {
            SoundEngine = new NAudioSoundEngine();
            SoundEngine.Init();
            SoundEngine.SetRecordCallback(SoundEngineRecordCallback);
        }

        private void SoundEngineRecordCallback(byte[] data)
        {
            if (GetAsyncKeyState(0x11) == 0 || Client.IsDisconnected) return;
            var voiceData = new VoiceData();
            voiceData.UserInfo = UserInfo;
            voiceData.SetDataFromBytes(data);
            WriteMessage(Message.Create(MessageType.Voice, voiceData));
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
                        SomeUserConnected(userInfo);
                        break;
                    case MessageType.UserDisconnected:
                        var userInfo2 = message.GetData<UserInfo>();
                        SomeUserDisconnected(userInfo2);
                        break;
                    case MessageType.CreateChannelResponse:
                        var channel = message.GetData<Channel>();
                        ChannelCreated(channel);
                        break;
                    case MessageType.GetAllChannelsResponse:
                        var channels = message.GetData<List<Channel>>();
                        RefreshChannels(channels);
                        break;
                    case MessageType.RenameChannelResponse:
                        ChannelRenamed(message.GetData<Channel>());
                        break;
                    case MessageType.UserChangeChannelResponse:
                        var userThatChangedChannel = message.GetData<UserInfo>();
                        SomeUserChangedChannel(userThatChangedChannel);
                        break;
                }
            }
        }

        private void HandleVoiceMessage(VoiceData voiceData)
        {
            SoundEngine.Play(voiceData.GetDataInBytes());
            SomeUserTalking(voiceData.UserInfo);
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
            Client.WriteMessage(Message.Create(MessageType.CredentialRequest, new UserCredential() { Username = username, Password = password }));
            var response = Client.ReadMessage().GetData<UserCredentialResponse>();
            if (response.IsAuthenticated)
            {
                UserInfo = response.UserInfo;

                Task.Factory.StartNew(() =>
                {
                    StartReading();
                }, TaskCreationOptions.LongRunning);

                SoundEngine.Record();
                Connected(true, response.Message);
                Client.WriteMessage(Message.Create(MessageType.GetAllChannelsRequest));
            }
            else
            {
                Connected(false, response.Message);
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
                Client.WriteMessage(Message.Create(MessageType.UserDisconnected, UserInfo));
            }

            SoundEngine.StopRecording();
            Client.Disconnect();
            Client = null;

            Disconnected();
        }

        [DllImport("User32.dll")]
        static extern short GetAsyncKeyState(int vKey);
    }
}

