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
using Babble.Core.Objects;
using System.Threading.Tasks;

namespace Client
{
    class VoiceClient
    {
        private NetworkClient networkClient;
        private ISoundEngine soundEngine;

        public event Action<bool, string> Connected;
        public event Action Disconnected;
        public event Action<Message> MessageReceived;

        public VoiceClient()
        {
            soundEngine = new NAudioSoundEngine();
            soundEngine.Init();
            soundEngine.SetRecordCallback(SoundEngineRecordCallback);
        }

        public UserSession UserSession { get { return networkClient == null ? null : networkClient.UserSession; } }
        public bool IsConnected { get { return networkClient == null ? false : networkClient.IsConnected; } }

        private void SoundEngineRecordCallback(byte[] data)
        {
            if (GetAsyncKeyState(0x11) == 0 || !IsConnected) return;
            var voiceData = new VoiceData();
            voiceData.UserSession = UserSession;
            voiceData.SetDataFromBytes(data);
            WriteMessage(Message.Create(MessageType.Voice, voiceData));
        }

        // HANDLER calls with null check

        private void OnConnected(bool connected, string responseMessage)
        {
            var handler = Connected;
            if (handler != null)
            {
                handler(connected, responseMessage);
            }
        }

        private void OnDisconnected()
        {
            var handler = Disconnected;
            if (handler != null)
            {
                handler();
            }
        }

        private void OnMessageReceived(Message message)
        {
            var handler = MessageReceived;
            if (handler != null)
            {
                handler(message);
            }
        }

        private Message ReadMessage()
        {
            return networkClient.ReadMessage();
        }

        public void WriteMessage(Message message)
        {
            networkClient.WriteMessage(message);
        }

        public void PlaySound(byte[] data)
        {
            soundEngine.Play(data);
        }

        private void StartReading()
        {
            while (IsConnected)
            {
                var message = ReadMessage();
                if (message == null)
                {
                    break;
                }

                OnMessageReceived(message);
            }
        }

        public void Connect(string host, int port, string username, string password)

        {
            if (networkClient != null)
            {
                Disconnect();
            }

            networkClient = NetworkClient.Connect(host, port);
            networkClient.WriteMessage(Message.Create(MessageType.CredentialRequest, new UserInfo() { Username = username, Password = password }));
            var response = networkClient.ReadMessage().GetData<UserCredentialResponse>();
            if (response.IsAuthenticated)
            {
                networkClient.UserSession = response.UserSession;

                Task.Factory.StartNew(() =>
                {
                    StartReading();

                    // handle disconnecting client if client is in bad state
                    Disconnect();
                }, TaskCreationOptions.LongRunning);

                soundEngine.Record();
                OnConnected(true, response.Message);
                networkClient.WriteMessage(Message.Create(MessageType.GetAllChannelsRequest));
            }
            else
            {
                OnConnected(false, response.Message);
                networkClient.Disconnect();
            }
        }

        public void Disconnect()
        {
            if (networkClient == null)
            {
                return;
            }

            if (IsConnected)
            {
                networkClient.WriteMessage(Message.Create(MessageType.UserDisconnected, UserSession));
            }

            soundEngine.StopRecording();
            networkClient.Disconnect();
            networkClient.UserSession = null;
            networkClient = null;

            OnDisconnected();
        }

        [DllImport("User32.dll")]
        static extern short GetAsyncKeyState(int vKey);
    }
}

