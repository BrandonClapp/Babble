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
            List<TcpClient> clientsList = new List<TcpClient>();

            TcpListener listener = new TcpListener(IPAddress.Any, 8888);
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("User Connected");
                clientsList.Add(client);
                new Thread(() =>
                {
                    try
                    {
                        byte[] buff = new byte[1646];

                        // while client is connected
                        for (int i; (i = client.GetStream().Read(buff, 0, buff.Length)) > 0; )
                        {
                            foreach (TcpClient c in clientsList)
                            {
                                if (c == client) continue;
                                c.GetStream().Write(buff, 0, buff.Length);
                                c.GetStream().Flush();
                            }
                        }
                    }
                    catch { }

                    clientsList.Remove(client);
                    client.Close();
                    Console.WriteLine("User Disconnected");

                }).Start();
            }
        }
    }
}
