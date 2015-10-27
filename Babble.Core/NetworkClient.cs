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
        private TcpClient tcpClient;
        private StreamReader reader;
        private StreamWriter writer;

        public NetworkClient(TcpClient tcpClient)
        {
            if (tcpClient == null) { throw new ArgumentNullException(nameof(tcpClient)); }

            this.tcpClient = tcpClient;
            reader = new StreamReader(tcpClient.GetStream());
            writer = new StreamWriter(tcpClient.GetStream());
            writer.AutoFlush = true;
        }

        public static NetworkClient Connect(string host, int port)
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect(host, port);
            NetworkClient networkClient = new NetworkClient(tcpClient);
            return networkClient;
        }

        public UserInfo ConnectedUser { get; set; }

        public bool IsConnected { get { return tcpClient.Connected; } }

        public void Disconnect()
        {
            tcpClient.Close();
        }

        public Message ReadMessage()
        {
            Message message = null;
            try
            {
                message = Message.FromJson(reader.ReadLine());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return message;
        }

        public void WriteMessage(Message message)
        {
            writer.WriteLine(message.ToJson());
        }
    }
}
