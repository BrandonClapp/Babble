using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core.Objects
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
}
