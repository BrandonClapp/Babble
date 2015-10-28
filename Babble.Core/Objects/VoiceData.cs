using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core.Objects
{
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
