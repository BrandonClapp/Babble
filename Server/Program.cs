using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start();
        }
    }

    class Server
    {
        public bool AcceptAnonymousLogins = true;
        public int Port = 8888;
        public IPAddress IPAddress = IPAddress.Any;
        public List<User> UserList = new List<User>();

        public void Start()
        {
            TcpListener listener = new TcpListener(IPAddress, Port);
            listener.Start();
            while (true)
            {
                User user = new User(listener.AcceptTcpClient());
                Console.WriteLine("User Connected");

                if (!AcceptAnonymousLogins)
                {

                }

                UserList.Add(user);
                user.Client.GetStream().BeginRead(user.Buffer, 0, user.Buffer.Length, ClientDataRecieved, user);
            }
        }


        private void ClientDataRecieved(IAsyncResult iar)
        {
            User user = iar.AsyncState as User;

            if (user.IsDisconnected)
            {
                UserList.Remove(user);
                user.Disconnect();
                Console.WriteLine("User Disconnected");
                return;
            }

            string json = UTF8Encoding.UTF8.GetString(user.Buffer).Trim('\0');
            //Console.WriteLine(json + " - " + json.Length);
            //dynamic message = JsonConvert.DeserializeObject<dynamic>(json);

            //switch (message.Type as string)
            //{
            //    case "Chat":
            //        BroadcastData(user, json, true);
            //        break;
            //    case "Voice":
            //        BroadcastData(user, json, false);
            //        break;
            //    case "Credentials":
            //        CredentialDataRecieved(user, message);
            //        break;
            //}

            user.Client.GetStream().BeginRead(user.Buffer, 0, user.Buffer.Length, new AsyncCallback(ClientDataRecieved), user);
        }

        private void CredentialDataRecieved(User user, dynamic message)
        {
            // Handle credential authorization
            Console.WriteLine(message.Username + " " + message.Password);
        }

        private void BroadcastData(User user, string json, bool includeSelf)
        {
            byte[] b = UTF8Encoding.UTF8.GetBytes(json);

            foreach (User u in UserList)
            {
                try
                {
                    if (!includeSelf && u == user) continue;
                    u.Client.GetStream().Write(b, 0, b.Length);
                    u.Client.GetStream().Flush();
                }
                catch { }
            }
        }
    }

    class User
    {
        public User(TcpClient client)
        {
            this.Client = client;
            this.Client.ReceiveBufferSize = 4096;
            Buffer = new byte[4096];
        }

        public TcpClient Client { get; set; }
        public byte[] Buffer { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public bool IsDisconnected
        {
            get
            {
                try
                {
                    return (Client.Client.Poll(0, SelectMode.SelectRead)
                      && Client.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                catch (SocketException se)
                {
                    return true;
                }
            }
        }
        public void Disconnect()
        {
            if (this.Client.Connected)
            {
                this.Client.GetStream().Close();
                this.Client.Close();
            }
        }
    }

    class Message
    {
        public string Type { get; set; }
    }

    class VoiceMessage : Message
    {
        public byte[] Buffer { get; set; }
    }

    class CredentialMessage : Message
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
