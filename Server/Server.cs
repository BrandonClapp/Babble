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
        private int Port = 8888;
        private IPAddress IPAddress = IPAddress.Any;
        private List<NetworkClient> ClientList = new List<NetworkClient>();

        public void Start()
        {
            TcpListener listener = new TcpListener(IPAddress, Port);
            listener.Start();
            while (true)
            {
                var client = new NetworkClient(listener.AcceptTcpClient());

                Console.WriteLine("User Connected");

                ClientList.Add(client);

                HandleConnectedClient(client);
            }
        }

        private void HandleConnectedClient(NetworkClient client)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (client.IsDisconnected)
                    {
                        client.Disconnect();
                        ClientList.Remove(client);
                        BroadcastData(client, Message.CreateUserDisconnectedMessage(client.UserInfo));
                        Console.WriteLine("User Disconnected");
                        return;
                    }

                    var message = client.ReadMessage();
                    if (message == null)
                    {
                        return;
                    }

                    switch (message.Type)
                    {
                        case MessageType.Chat:
                            BroadcastData(client, message, true);
                            break;
                        case MessageType.Voice:
                            BroadcastData(client, message, true);
                            break;
                        case MessageType.Credentials:
                            CredentialDataReceived(client, message);
                            break;
                        case MessageType.Hello:
                            HelloReceived(client);
                            break;
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void HelloReceived(NetworkClient client)
        {
            Console.WriteLine("hello");
            client.WriteMessage(Message.CreateChannelCreatedMessage(new Channel { Name = "Default Channel", Id = 0 }));
            client.WriteMessage(Message.CreateChannelCreatedMessage(new Channel { Name = "Another Channel", Id = 1 }));
            client.WriteMessage(Message.CreateChannelCreatedMessage(new Channel { Name = "Again Channel", Id = 2 }));

            BroadcastData(client, Message.CreateUserConnectedMessage(client.UserInfo), true);
        }

        private void CredentialDataReceived(NetworkClient client, Message message)
        {
            // Handle credential authorization
            if (string.IsNullOrWhiteSpace(message.Data.Username.Value))
            {
                message.Data.Username.Value = "Anon";
                message.Data.Password.Value = string.Empty;
            }
            client.UserInfo = new UserInfo();
            client.UserInfo.Username = message.Data.Username.Value as string;
            client.UserInfo.Password = message.Data.Password.Value as string;

            Console.WriteLine(client.UserInfo.Username + " " + client.UserInfo.Password);
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
                    Console.WriteLine("Broadcasting: " + message.Type);
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
