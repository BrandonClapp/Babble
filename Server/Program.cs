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
            List<TcpClient> clientList = new List<TcpClient>();

            TcpListener listener = new TcpListener(IPAddress.Any, 8888);
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                clientList.Add(client);
                new Thread(() =>
                    {
                        try
                        {
                            Console.WriteLine("User Connected");
                            while (true)
                            {
                                // send each clients stream to every other client in clientList.
                                // ping user with packet every so often to make sure they're still online
                            }
                        }
                        catch
                        {
                            clientList.Remove(client);
                            Console.WriteLine("User Disconnected");
                        }
                    }).Start();
            }
        }
    }
}
