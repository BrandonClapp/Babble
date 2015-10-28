using Babble.Core;
using Babble.Core.Objects;
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
        private List<ChannelSession> channelSessions = new List<ChannelSession>();
        private Dictionary<MessageType, Action<NetworkClient, Message>> messageHandlers = new Dictionary<MessageType, Action<NetworkClient, Message>>();

        private DatabaseService databaseService = new DatabaseService();
        private ChannelService channelService = new ChannelService();
        private UserService userService = new UserService();

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
            this.channelSessions.Clear();

            var channelSessions = channelService.GetAllChannels().Select(c => new ChannelSession(c));
            this.channelSessions.AddRange(channelSessions);
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
            try
            {
                var credential = message.GetData<UserCredential>();
                // Handle credential authorization
                if (string.IsNullOrWhiteSpace(credential.Username))
                {
                    var userSession = new UserSession();
                    userSession.ConnectionId = Guid.NewGuid();
                    userSession.UserInfo = new UserInfo() { Username = "Anon#" + new Random().Next(5000) };
                    client.UserSession = userSession;
                    AddUserToChannel(userSession, 0);

                    var response = CreateCredentialSuccessResponse(userSession);
                    BroadcastAll(client, Message.Create(MessageType.UserConnected, userSession));
                    client.WriteMessage(Message.Create(MessageType.CredentialResponse, response));
                }
                else if (userService.IsUserAuthenticated(credential.Username, credential.Password))
                {
                    var userSession = new UserSession();
                    userSession.ConnectionId = Guid.NewGuid();
                    userSession.UserInfo = new UserInfo() { Username = "Anon#" + new Random().Next(5000) };
                    client.UserSession = userSession;
                    AddUserToChannel(userSession, 0);

                    var response = CreateCredentialSuccessResponse(userSession);
                    BroadcastAll(client, Message.Create(MessageType.UserConnected, userSession));
                    client.WriteMessage(Message.Create(MessageType.CredentialResponse, response));
                }
                else
                {
                    var response = CreateCredentialFailResponse();
                    client.WriteMessage(Message.Create(MessageType.CredentialResponse, response));
                }
            }
            catch
            {
                var response = CreateCredentialFailResponse();
                client.WriteMessage(Message.Create(MessageType.CredentialResponse, response));
            }
        }

        private UserCredentialResponse CreateCredentialSuccessResponse(UserSession userSession)
        {
            var response = new UserCredentialResponse();
            response.UserSession = userSession;
            response.IsAuthenticated = true;
            response.Message = "Great success!";
            return response;
        }

        private UserCredentialResponse CreateCredentialFailResponse()
        {
            var response = new UserCredentialResponse();
            response.IsAuthenticated = false;
            response.Message = "Brandon fix this!";
            return response;
        }

        private void HelloHandler(NetworkClient client, Message message)
        {
            // TODO: see what we are using this for
        }

        private void GetAllChannelsRequestHandler(NetworkClient client, Message message)
        {
            client.WriteMessage(Message.Create(MessageType.GetAllChannelsResponse, channelSessions));
        }

        private void UserChangeChannelRequestHandler(NetworkClient client, Message message)
        {
            RemoveUserFromChannel(client.UserSession);

            // todo: validation that the user can join target channel.
            AddUserToChannel(client.UserSession, (int)message.Data);

            BroadcastAll(client, Message.Create(MessageType.UserChangeChannelResponse, client.UserSession), true);
        }

        private void CreateChannelRequestHandler(NetworkClient client, Message message)
        {
            var channelToCreate = message.GetData<Channel>();
            var createdChannel = channelService.CreateChannel(channelToCreate.Name);
            var createdChannelSession = new ChannelSession(createdChannel);
            AddChannel(createdChannelSession);
            BroadcastAll(client, Message.Create(MessageType.CreateChannelResponse, createdChannelSession), true);
        }

        private void RenameChannelRequestHandler(NetworkClient client, Message message)
        {
            var channelFromRequest = message.GetData<Channel>();
            var channelFromServer = channelSessions.FirstOrDefault(c => c.Channel.Id == channelFromRequest.Id);
            if (channelFromServer == null)
            {
                Console.WriteLine("Unable to find channel id {0} in server", channelFromRequest.Id);
                return;
            }
            channelService.UpdateChannel(channelFromRequest); // ensure you can update in database first
            channelFromServer.Channel.Name = channelFromRequest.Name; // then update the channel object currently serving
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

            var channelFromServer = channelSessions.FirstOrDefault(c => c.Channel.Id == channelFromRequest.Id);
            if (channelFromServer == null)
            {
                Console.WriteLine("Unable to find channel id {0} in server", channelFromRequest.Id);
                return;
            }

            channelService.DeleteChannel(channelFromServer.Channel.Id);

            foreach (var userSession in channelFromServer.UserSessions)
            {
                AddUserToChannel(userSession, 0);
            }
            channelSessions.Remove(channelFromServer);

            BroadcastAll(client, Message.Create(MessageType.GetAllChannelsResponse, channelSessions), true);
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
            BroadcastAll(client, Message.Create(MessageType.UserDisconnected, client.UserSession));
            client.Disconnect();
            connectedClients.Remove(client);

            // refactor this
            RemoveUserFromChannel(client.UserSession);
            Console.WriteLine("User Disconnected: {0}, now you have {1} users connected", client.UserSession.UserInfo.Username, connectedClients.Count);
        }

        private void AddChannel(ChannelSession channel)
        {
            channelSessions.Add(channel);
        }

        private void AddUserToChannel(UserSession userSession, int target)
        {
            var channel = channelSessions.FirstOrDefault(c => c.Channel.Id == target);
            if (channel == null)
            {
                // "Default Channel" configurable at a later time.
                // for now, just the first one.
                channelSessions[0].AddUser(userSession);
            }
            else
            {
                channel.AddUser(userSession);
                //channel.Users.Add(userInfo);
            }
        }

        private void RemoveUserFromChannel(UserSession userSession)
        {
            var source = channelSessions.Find(ch => ch.Channel.Id == userSession.ChannelId);
            var user = source.UserSessions.Find(u => u.ConnectionId == userSession.ConnectionId);
            source.RemoveUser(user);
        }

        private void BroadcastAll(NetworkClient sourceClient, Message message, bool includeSelf = false)
        {
            Broadcast(sourceClient, connectedClients, message, includeSelf);
        }

        private void BroadcastChannel(NetworkClient sourceClient, Message message, bool includeSelf = false)
        {
            var channelId = sourceClient.UserSession.ChannelId;
            var channel = channelSessions.FirstOrDefault(c => c.Channel.Id == channelId);
            if (channel == null)
            {
                Console.WriteLine("Unable to find channel id {0} to broadcast", channelId);
                return;
            }

            var targetClients = from client in connectedClients
                                     join user in channel.UserSessions on client.UserSession.ConnectionId equals user.ConnectionId
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
                        sourceClient.UserSession.UserInfo.Username,
                        sourceClient.UserSession.ChannelId);

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
