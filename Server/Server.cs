using Babble.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Server.Services;

namespace Server
{
    class Server
    {
        private const int LobbyChannelId = 1;
        private readonly int Port = 8888;
        private readonly IPAddress IPAddress = IPAddress.Any;
        private List<NetworkClient> connectedClients = new List<NetworkClient>();
        private List<Channel> channels = new List<Channel>();
        private Dictionary<MessageType, Action<NetworkClient, Message>> messageHandlers = new Dictionary<MessageType, Action<NetworkClient, Message>>();

        private DatabaseService databaseService = new DatabaseService();
        private ChannelService channelService = new ChannelService();

        public Server()
        {
            InitMessageHandlers();

            InitDatabase();
            InitDefaultChannels();
        }

        public void Start()
        {

            TcpListener listener = new TcpListener(IPAddress, Port);
            listener.Start();
            while (true)
            {
                var client = new NetworkClient(listener.AcceptTcpClient());
                connectedClients.Add(client);

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        HandleConnectedClient(client);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                },TaskCreationOptions.LongRunning);

                Console.WriteLine("User Connected. Now you have {0} users connected", connectedClients.Count);
            }
        }

        

        private void InitMessageHandlers()
        {
            messageHandlers.Clear();
            messageHandlers.Add(MessageType.Chat, ChatHandler);
            messageHandlers.Add(MessageType.Voice, VoiceHandler);
            messageHandlers.Add(MessageType.CredentialRequest, CredentialRequestHandler);
            messageHandlers.Add(MessageType.Hello, HelloHandler);
            messageHandlers.Add(MessageType.GetAllChannelsRequest, GetAllChannelsRequestHandler);
            messageHandlers.Add(MessageType.UserChangeChannelRequest, UserChangeChannelRequestHandler);
            messageHandlers.Add(MessageType.CreateChannelRequest, CreateChannelRequestHandler);
            messageHandlers.Add(MessageType.RenameChannelRequest, RenameChannelRequestHandler);
            messageHandlers.Add(MessageType.DeleteChannelRequest, DeleteChannelRequestHandler);
        }

        private void InitDatabase()
        {
            databaseService.InitDatabase();
        }

        private void InitDefaultChannels()
        {
            channels.Clear();

            var channelsFromDb = channelService.GetAllChannels();
            channels.AddRange(channelsFromDb);
        }

        private void ChatHandler(NetworkClient client, Message message)
        {
            BroadcastChannel(client, message, true);
        }

        private void VoiceHandler(NetworkClient client, Message message)
        {
            BroadcastChannel(client, message, true);
        }

        private void CredentialRequestHandler(NetworkClient client, Message message)
        {
            var credential = message.GetData<UserCredential>();
            var userInfo = new UserInfo();
            var response = new UserCredentialResponse();
            response.UserInfo = userInfo;
            // Handle credential authorization
            if (string.IsNullOrWhiteSpace(credential.Username))
            {
                userInfo.Id = Guid.NewGuid();
                userInfo.Username = "Anon#" + new Random().Next(5000);
                client.ConnectedUser = userInfo;

                response.IsAuthenticated = true;
                response.Message = "Great success!";
                AddUserToChannel(userInfo, 0);
                BroadcastAll(client, Message.Create(MessageType.UserConnected, userInfo));
            }
            else
            {
                // TODO: handle actual username and password

                response.IsAuthenticated = false;
                response.Message = "Brandon fix this!";
            }

            client.WriteMessage(Message.Create(MessageType.CredentialResponse, response));
        }

        private void HelloHandler(NetworkClient client, Message message)
        {
            // TODO: see what we are using this for
        }

        private void GetAllChannelsRequestHandler(NetworkClient client, Message message)
        {
            client.WriteMessage(Message.Create(MessageType.GetAllChannelsResponse, channels));
        }

        private void UserChangeChannelRequestHandler(NetworkClient client, Message message)
        {
            RemoveUserFromChannel(client.ConnectedUser);

            // todo: validation that the user can join target channel.
            AddUserToChannel(client.ConnectedUser, (int)message.Data);

            BroadcastAll(client, Message.Create(MessageType.UserChangeChannelResponse, client.ConnectedUser), true);
        }

        private void CreateChannelRequestHandler(NetworkClient client, Message message)
        {
            var channelToCreate = message.GetData<Channel>();
            var createdChannel = channelService.CreateChannel(channelToCreate.Name);
            AddChannel(createdChannel);
            BroadcastAll(client, Message.Create(MessageType.CreateChannelResponse, createdChannel), true);
        }

        private void RenameChannelRequestHandler(NetworkClient client, Message message)
        {
            var channelFromRequest = message.GetData<Channel>();
            var channelFromServer = channels.FirstOrDefault(c => c.Id == channelFromRequest.Id);
            if (channelFromServer == null)
            {
                Console.WriteLine("Unable to find channel id {0} in server", channelFromRequest.Id);
                return;
            }
            channelService.UpdateChannel(channelFromRequest); // ensure you can update in database first
            channelFromServer.Name = channelFromRequest.Name; // then update the channel object currently serving
            BroadcastAll(client, Message.Create(MessageType.RenameChannelResponse, channelFromRequest), true);
        }

        private void DeleteChannelRequestHandler(NetworkClient client, Message message)
        {
            var channelFromRequest = message.GetData<Channel>();

            if (channelFromRequest.Id == LobbyChannelId)
            {
                Console.WriteLine("Cannot delete designated lobby channel id " + LobbyChannelId);
                return;
            }

            var channelFromServer = channels.FirstOrDefault(c => c.Id == channelFromRequest.Id);
            if (channelFromServer == null)
            {
                Console.WriteLine("Unable to find channel id {0} in server", channelFromRequest.Id);
                return;
            }

            channelService.DeleteChannel(channelFromServer.Id);

            foreach (var user in channelFromServer.Users)
            {
                AddUserToChannel(user, 0);
            }
            channels.Remove(channelFromServer);

            BroadcastAll(client, Message.Create(MessageType.GetAllChannelsResponse, channels), true);
        }

        private void HandleConnectedClient(NetworkClient client)
        {
            while (client.IsConnected)
            {
                var message = client.ReadMessage();
                if (message == null)
                {
                    break;
                }

                Action<NetworkClient, Message> handler = null;
                messageHandlers.TryGetValue(message.Type, out handler);
                if (handler == null)
                {
                    Console.WriteLine("Error, unable to find a message handler for message type: {0}", message.Type);
                    continue;
                }

                handler(client, message);
            }

            // If the handler no longer running, do some clean up here
            BroadcastAll(client, Message.Create(MessageType.UserDisconnected, client.ConnectedUser));
            client.Disconnect();
            connectedClients.Remove(client);

            // refactor this
            RemoveUserFromChannel(client.ConnectedUser);
            Console.WriteLine("User Disconnected: {0}, now you have {1} users connected", client.ConnectedUser.Username, connectedClients.Count);
        }

        private void AddChannel(Channel channel)
        {
            channels.Add(channel);
        }

        private void AddUserToChannel(UserInfo userInfo, int target)
        {
            var channel = channels.FirstOrDefault(c => c.Id == target);
            if (channel == null)
            {
                // "Default Channel" configurable at a later time.
                // for now, just the first one.
                channels[0].AddUser(userInfo);
            }
            else
            {
                channel.AddUser(userInfo);
                //channel.Users.Add(userInfo);
            }
        }

        private void RemoveUserFromChannel(UserInfo userInfo)
        {
            var source = channels.Find(ch => ch.Id == userInfo.ChannelId);
            var user = source.Users.Find(u => u.Id == userInfo.Id);
            source.RemoveUser(user);
        }

        private void BroadcastAll(NetworkClient sourceClient, Message message, bool includeSelf = false)
        {
            Broadcast(sourceClient, connectedClients, message, includeSelf);
        }

        private void BroadcastChannel(NetworkClient sourceClient, Message message, bool includeSelf = false)
        {
            var channelId = sourceClient.ConnectedUser.ChannelId;
            var channel = channels.FirstOrDefault(c => c.Id == channelId);
            if (channel == null)
            {
                Console.WriteLine("Unable to find channel id {0} to broadcast", channelId);
                return;
            }

            var targetClients = from client in connectedClients
                                     join user in channel.Users on client.ConnectedUser.Id equals user.Id
                                     select client;
            
            if (targetClients.Any())
            {
                Broadcast(sourceClient, targetClients.ToList(), message, includeSelf);
            }
        }

        private void Broadcast(NetworkClient sourceClient, List<NetworkClient> targetClients, Message message, bool includeSelf = false)
        {
            Parallel.ForEach(targetClients, (c) =>
            {
                try
                {
                    if (!includeSelf && sourceClient == c)
                    {
                        return;
                    }

                    Console.WriteLine("Broadcasting: {0} from user {1} in channel {2}", 
                        message.Type, 
                        sourceClient.ConnectedUser.Username,
                        sourceClient.ConnectedUser.ChannelId);

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
