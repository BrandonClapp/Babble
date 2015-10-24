using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core
{
    public class NetworkClient
    {
        public NetworkClient(TcpClient client)
        {
            Client = client;
            Reader = new StreamReader(Client.GetStream());
            Writer = new StreamWriter(Client.GetStream());
            Writer.AutoFlush = true;
        }

        public static NetworkClient Connect(string host, int port)
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect(host, port);
            NetworkClient client = new NetworkClient(tcpClient);
            return client;
        }

        private TcpClient Client { get; set; }
        private StreamReader Reader { get; set; }
        private StreamWriter Writer { get; set; }
        public UserInfo UserInfo { get; set; }

        public bool IsDisconnected
        {
            get
            {
                return !Client.Connected;
            }
        }

        public void Disconnect()
        {
            Client.Close();
        }

        public Message ReadMessage()
        {
            Message message = null;
            try
            {
                message = Message.FromJson(Reader.ReadLine());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return message;
        }

        public void WriteMessage(Message message)
        {
            Writer.WriteLine(message.ToJson());
        }
    }
}
