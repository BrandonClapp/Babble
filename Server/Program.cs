using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

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

            //string path = @"C:\Users\brandonclapp\Desktop\Voice-Meeting-master\Client\bin\Debug\Client.exe";
            //Process.Start(path);

            while (true)
            {
                User user = new User(listener.AcceptTcpClient());
                Console.WriteLine("User Connected");

                if (!AcceptAnonymousLogins)
                {

                }

                UserList.Add(user);
                user.Client.GetStream().BeginRead(user.Buffer, user.Offset, user.Buffer.Length, ClientDataRecieved, user);
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

            user.Offset += user.Client.GetStream().EndRead(iar);
            try
            {
                while (user.Offset >= BitConverter.ToUInt16(user.Buffer, 0) + 2)
                {
                    ushort messageLength = BitConverter.ToUInt16(user.Buffer, 0);

                    string json = UTF8Encoding.UTF8.GetString(user.Buffer, 2, messageLength);

                    user.Offset -= (messageLength + 2);
                    Array.Copy(user.Buffer, messageLength + 2, user.Buffer, 0, user.Offset);

                    dynamic message = JsonConvert.DeserializeObject<dynamic>(json);

                    //Console.WriteLine(message.Type);
                    switch (message.Type.Value as string)
                    {
                        case "Chat":
                            BroadcastData(user, json, true);
                            break;
                        case "Voice":
                            BroadcastData(user, json, true);
                            break;
                        case "Credentials":
                            CredentialDataRecieved(user, message);
                            break;
                    }
                }
                user.Client.GetStream().BeginRead(user.Buffer, user.Offset, user.Buffer.Length - user.Offset, new AsyncCallback(ClientDataRecieved), user);
            }
            catch
            {
            }
        }

        private void CredentialDataRecieved(User user, dynamic message)
        {
            // Handle credential authorization
            if (string.IsNullOrWhiteSpace(message.Username.Value))
            {
                message.Username = "Anon";
                message.Password = string.Empty;
            }
            Console.WriteLine(message.Username + " " + message.Password);
        }

        private void BroadcastData(User user, string json, bool includeSelf)
        {
            foreach (User u in UserList)
            {
                try
                {
                    if (!includeSelf && u == user) continue;
                    u.WriteMessage(json);
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
            Buffer = new byte[1<<14];
        }

        public TcpClient Client { get; set; }
        public byte[] Buffer { get; set; }
        public int Offset { get; set; }
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
                catch
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

        public void WriteMessage(string json)
        {
            byte[] message = UTF8Encoding.UTF8.GetBytes(json);
            lock (WriteLock)
            {
                Client.GetStream().Write(BitConverter.GetBytes((short)message.Length), 0, 2); // first two bytes
                Client.GetStream().Write(message, 0, message.Length); // string of json
                Client.GetStream().Flush();
            }
        }

        private object WriteLock = new object();
    }
}
