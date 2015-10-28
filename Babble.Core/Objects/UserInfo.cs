using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core.Objects
{
    public class UserInfo
    {
        public Guid ConnectionId { get; set; }
        public int Id { get; set; }
        public string Username { get; set; }
        public bool Authenticated { get; set; }
        public int ChannelId { get; set; }
    }

    

    
}
