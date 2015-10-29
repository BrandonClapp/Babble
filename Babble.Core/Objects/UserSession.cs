using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core.Objects
{
    public class UserSession
    {
        public Guid ConnectionId { get; set; }
        public bool Authenticated { get; set; }
        public int ChannelId { get; set; }
        public UserInfo UserInfo { get; set; }
        public UserStatus UserStatus { get; set; }
    }
}
