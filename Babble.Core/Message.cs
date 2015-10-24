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
        public Message()
        {
        }

        public Message(MessageType type, dynamic data)
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

        public static Message CreateCredentialsMessage(UserInfo userInfo)
        {
            var message = new Message(MessageType.Credentials, userInfo);
            return message;
        }

        public static Message CreateHelloMessage()
        {
            return new Message(MessageType.Hello, null);
        }

        public static Message CreateChannelCreatedMessage(Channel channel)
        {
            var message = new Message(MessageType.ChannelCreated, channel);
            return message;
        }

        public static Message CreateUserConnectedMessage(UserInfo userInfo)
        {
            var message = new Message(MessageType.UserConnected, userInfo);
            return message;
        }

        public static Message CreateUserDisconnectedMessage(UserInfo userInfo)
        {
            var message = new Message(MessageType.UserDisconnected, userInfo);
            return message;
        }

        public static Message CreateVoiceMessage(object data)
        {
            var message = new Message(MessageType.Voice, data);
            return message;
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
