using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                        // Check credentials with database.
                        if (false) // if credentials are not valid
                        {
                            user.Client.Close();
                            continue;
                        }
                    }

                    UserList.Add(user);
                    user.Client.GetStream().BeginRead(user.Buffer, 0, user.Buffer.Length, new AsyncCallback(ClientRecieved), user);


                }

            }

            private void ClientRecieved(IAsyncResult iar)
            {
                User user = iar.AsyncState as User;



                //bool IsYouConnectionBroken =
                //user.Client.Client.Poll(1, SelectMode.SelectWrite) &&
                //user.Client.Client.Poll(1, SelectMode.SelectRead) && 
                //!user.Client.Client.Poll(1, SelectMode.SelectError);

                Console.WriteLine(user.Client.Client.Poll(1, SelectMode.SelectWrite | SelectMode.SelectRead | SelectMode.SelectError));

                //Console.WriteLine(IsYouConnectionBroken);
                List<User> baddies = new List<User>();

                foreach (User u in UserList)
                {
                    try
                    {
                        if (u == user) continue;
                        u.Client.GetStream().Write(user.Buffer, 0, user.Buffer.Length);
                        u.Client.GetStream().Flush();
                    }
                    catch
                    {
                        baddies.Add(u);
                        u.Client.Close();
                        Console.WriteLine("User Disconnected");
                    }
                }

                UserList = UserList.Except(baddies).ToList();

                user.Client.GetStream().BeginRead(user.Buffer, 0, user.Buffer.Length, new AsyncCallback(ClientRecieved), user);
            }
        }

        class User
        {
            public TcpClient Client { get; set; }
            public byte[] Buffer { get; set; }
        }
    }
}
