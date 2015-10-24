using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core
{
    public class Message
    {

        [JsonConstructor]
        private Message(MessageType type, dynamic data = null)
        {
            Type = type;
            Data = data;
        }

        public MessageType Type { get; set; }
        public dynamic Data { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Message FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Message>(json);
        }

        public static Message Create(MessageType type, dynamic data = null)
        {
            return new Message(type, data);
        }
    }

    public enum MessageType
    {
        None,
        Credentials,
        Hello,
        Chat,
        Voice,
        ChannelCreated,
        UserConnected,
        UserDisconnected
    }

    public class UserInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int ChannelId { get; set; }
    }

    public class Channel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
