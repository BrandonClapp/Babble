﻿using Babble.Core;
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

                Task.Factory.StartNew(() =>
                {
                    HandleConnectedClient(client);
                },TaskCreationOptions.LongRunning);

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
                    case MessageType.CredentialRequest:
                        CredentialDataReceived(client, message);
                        break;
                    case MessageType.Hello:
                        HelloReceived(client);
                        break;
                    case MessageType.GetAllChannelsRequest:
                        client.WriteMessage(Message.Create(MessageType.GetAllChannelsResponse, Channels));
                        break;
                    case MessageType.UserChangeChannelRequest:
                        ChangeChannelRequestReceived(client, message);
                        break;
                    case MessageType.CreateChannelRequest:
                        var channel = message.GetData<Channel>();
                        channel.Id = Channels.Select(c => c.Id).Max() + 1;
                        AddChannel(channel);
                        BroadcastData(client, Message.Create(MessageType.CreateChannelResponse, channel), true);
                        break;
                    case MessageType.RenameChannelRequest:
                        RenameChannelRequestReceived(client, message);
                        break;
                }
            }

            // If the handler no longer running, do some clean up here
            BroadcastData(client, Message.Create(MessageType.UserDisconnected, client.UserInfo));
            client.Disconnect();
            ClientList.Remove(client);

            // refactor this
            RemoveUserFromChannel(client.UserInfo);
            Console.WriteLine("User Disconnected: {0}, now you have {1} users connected", client.UserInfo.Username, ClientList.Count);
        }

        private void ChangeChannelRequestReceived(NetworkClient client, Message message)
        {
            RemoveUserFromChannel(client.UserInfo);

            // todo: validation that the user can join target channel.
            AddUserToChannel(client.UserInfo, (int)message.Data);

            BroadcastData(client, Message.Create(MessageType.UserChangeChannelResponse, client.UserInfo), true);
        }

        private void RenameChannelRequestReceived(NetworkClient client, Message message)
        {
            var channelFromRequest = message.GetData<Channel>();
            var channelFromServer = Channels.FirstOrDefault(c => c.Id == channelFromRequest.Id);
            if (channelFromServer == null)
            {
                Console.WriteLine($"Unable to find channel id {channelFromRequest.Id} in server");
                return;
            }
            channelFromServer.Name = channelFromRequest.Name;
            BroadcastData(client, Message.Create(MessageType.RenameChannelResponse, channelFromRequest), true);
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
            var response = new UserCredentialResponse();
            response.UserInfo = userInfo;
            // Handle credential authorization
            if (string.IsNullOrWhiteSpace(credential.Username))
            {
                userInfo.Id = Guid.NewGuid();
                userInfo.Username = "Anon#" + new Random().Next(5000);
                response.IsAuthenticated = true;
                response.Message = "Great success!";

                AddUserToChannel(userInfo, 0);
                BroadcastData(client, Message.Create(MessageType.UserConnected, userInfo));
            }
            else
            {
                // TODO: handle actual username and password

                response.IsAuthenticated = false;
                response.Message = "Brandon fix this!";
            }

            client.WriteMessage(Message.Create(MessageType.CredentialResponse, response));
        }

        private void AddChannel(Channel channel)
        {
            Channels.Add(channel);
        }

        private void AddUserToChannel(UserInfo userInfo, int target)
        {
            var channel = Channels.FirstOrDefault(c => c.Id == target);
            if (channel == null)
            {
                // "Default Channel" configurable at a later time.
                // for now, just the first one.
                Channels[0].AddUser(userInfo);
            }
            else
            {
                channel.AddUser(userInfo);
                //channel.Users.Add(userInfo);
            }
        }

        private void RemoveUserFromChannel(UserInfo userInfo)
        {
            var source = Channels.Find(ch => ch.Id == userInfo.ChannelId);
            var user = source.Users.Find(u => u.Id == userInfo.Id);
            source.RemoveUser(user);
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
