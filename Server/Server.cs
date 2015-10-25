using Babble.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        private readonly int Port = 8888;
        private readonly IPAddress IPAddress = IPAddress.Any;
        private readonly List<NetworkClient> ClientList = new List<NetworkClient>();
        private readonly List<Channel> Channels = new List<Channel>();

        public void Start()
        {
            InitDefaultChannels();

            TcpListener listener = new TcpListener(IPAddress, Port);
            listener.Start();
            while (true)
            {
                var client = new NetworkClient(listener.AcceptTcpClient());
                ClientList.Add(client);
                HandleConnectedClient(client);

                Console.WriteLine("User Connected. Now you have {0} users connected", ClientList.Count);
            }
        }

        // Init default channels
        private void InitDefaultChannels()
        {
            Channels.Clear();
            Channels.Add(new Channel { Name = "Default Channel", Id = 0 });
            Channels.Add(new Channel { Name = "Another Channel", Id = 1 });
            Channels.Add(new Channel { Name = "Again Channel", Id = 2 });
        }

        private void HandleConnectedClient(NetworkClient client)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var message = client.ReadMessage();
                    if (message == null)
                    {
                        break;
                    }

                    switch (message.Type)
                    {
                        // todo: refactor
                        case MessageType.Chat:
                            BroadcastData(client, message, true);
                            break;
                        case MessageType.Voice:
                            BroadcastData(client, message, true);
                            break;
                        case MessageType.Credential:
                            CredentialDataReceived(client, message);
                            break;
                        case MessageType.Hello:
                            HelloReceived(client);
                            break;
                        case MessageType.RequestChannels:
                            client.WriteMessage(Message.Create(MessageType.RequestChannels, Channels));
                            break;
                        case MessageType.RequestChannelCreate:
                            var channel = message.GetData<Channel>();
                            channel.Id = Channels.Select(c => c.Id).Max() + 1;
                            AddChannel(channel);
                            BroadcastData(client, Message.Create(MessageType.ChannelCreated, channel), true);
                            break;
                    }
                }

                // If the handler no longer running, do some clean up here
                BroadcastData(client, Message.Create(MessageType.UserDisconnected, client.UserInfo));
                client.Disconnect();
                ClientList.Remove(client);
                RemoveUserFromChannel(client.UserInfo);
                Console.WriteLine("User Disconnected: {0}, now you have {1} users connected", client.UserInfo.Username, ClientList.Count);
            }, TaskCreationOptions.LongRunning);
        }

        private void HelloReceived(NetworkClient client)
        {
            // Do nothing for now
        }

        private void CredentialDataReceived(NetworkClient client, Message message)
        {
            var credential = message.GetData<UserCredential>();
            var userInfo = new UserInfo();
            client.UserInfo = userInfo;
            var result = new UserCredentialResult();
            // Handle credential authorization
            if (string.IsNullOrWhiteSpace(credential.Username))
            {
                userInfo.Username = "Anon#" + new Random().Next(5000);
                result.IsAuthenticated = true;
                result.Message = "Great success!";

                AddUserToChannel(userInfo);
                BroadcastData(client, Message.Create(MessageType.UserConnected, userInfo));
            }
            else
            {
                // TODO: handle actual username and password

                result.IsAuthenticated = false;
                result.Message = "Brandon fix this!";
            }

            client.WriteMessage(Message.Create(MessageType.CredentialResult, result));
        }

        private void AddChannel(Channel channel)
        {
            Channels.Add(channel);
        }

        private void AddUserToChannel(UserInfo userInfo)
        {
            var channel = Channels.FirstOrDefault(c => c.Id == userInfo.ChannelId);
            if (channel == null)
            {
                Channels[0].Users.Add(userInfo);
            }
            else
            {
                channel.Users.Add(userInfo);
            }
        }

        private void RemoveUserFromChannel(UserInfo userInfo)
        {
            foreach (var channel in Channels)
            {
                channel.Users.RemoveAll(u => u.Username == userInfo.Username);
            }
        }

        private void BroadcastData(NetworkClient client, Message message, bool includeSelf = false)
        {
            Parallel.ForEach(ClientList, (c) =>
            {
                try
                {
                    if (!includeSelf && client == c)
                    {
                        return;
                    }

                    Console.WriteLine("Broadcasting: {0}", message.Type);

                    c.WriteMessage(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
    }
}
