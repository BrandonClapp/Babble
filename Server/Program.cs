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
                    User user = new User { Client = listener.AcceptTcpClient(), Buffer = new byte[1646] };
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
                VoiceDataRecieved(iar);
            }

            private void CredentialDataRecieved(IAsyncResult iar)
            {
                
            }

            private void VoiceDataRecieved(IAsyncResult iar)
            {
                User user = iar.AsyncState as User;

                foreach (User u in UserList)
                {
                    try
                    {
                        if (u == user) continue;
                        u.Client.GetStream().Write(user.Buffer, 0, user.Buffer.Length);
                        u.Client.GetStream().Flush();
                    }
                    catch { }
                }

                if (user.IsDisconnected)
                {
                    UserList.Remove(user);
                    user.Disconnect();
                    Console.WriteLine("User Disconnected");
                }
                else user.Client.GetStream().BeginRead(user.Buffer, 0, user.Buffer.Length, new AsyncCallback(ClientDataRecieved), user);
            }
        }

        class User
        {
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
    }
}
