using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core
{
    public enum MessageType
    {
        None,
        CredentialRequest,
        CredentialResponse,
        Hello,
        Chat,
        Voice,
        GetAllChannelsRequest,
        GetAllChannelsResponse,
        CreateChannelRequest,
        CreateChannelResponse,
        UserConnected,
        UserDisconnected,
        UserChangeChannelRequest,  // maybe this should be renamed
        UserChangeChannelResponse
    }

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

        public T GetData<T>() where T : class
        {
            // if data is already that type, return
            var convertedData = Data as T;
            if (convertedData != null)
            {
                return convertedData;
            }

            // otherwise try and see if data is being serialized as JObject
            // this happens when you send it accross the wire
            var jObjectData = Data as Newtonsoft.Json.Linq.JObject;
            if (jObjectData != null)
            {
                return jObjectData.ToObject<T>();
            }

            var jArrayData = Data as Newtonsoft.Json.Linq.JArray;
            if (jArrayData != null)
            {
                return jArrayData.ToObject<T>();
            }

            // otherwise just return data itself
            return Data;
        }

        public static Message Create(MessageType type, dynamic data = null)
        {
            return new Message(type, data);
        }

        public string ToJson()
        {
            var json = JsonConvert.SerializeObject(this);
            return json;
        }

        public static Message FromJson(string json)
        {
            var message = JsonConvert.DeserializeObject<Message>(json);
            return message;
        }
    }

    public class UserInfo
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public bool Authenticated { get; set; }
        public int ChannelId { get; set; }
    }

    public class UserCredential
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserCredentialResponse
    {
        public bool IsAuthenticated { get; set; }
        public string Message { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class Channel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<UserInfo> Users { get; private set; } = new List<UserInfo>();

        public void AddUser(UserInfo user)
        {
            // validation logic for adding user here
            Users.Add(user);
            user.ChannelId = Id;
        }

        public void RemoveUser(UserInfo user)
        {
            Users.Remove(user);
            user.ChannelId = -1;
        }
    }

    public class VoiceData
    {
        public UserInfo UserInfo { get; set; }
        public string Data { get; set; } = string.Empty;
        public void SetDataFromBytes(byte[] bytes)
        {
            Data = Convert.ToBase64String(bytes);
        }
        public byte[] GetDataInBytes()
        {
            return Convert.FromBase64String(Data);
        }
    }
}
