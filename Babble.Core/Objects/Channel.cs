using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core.Objects
{
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
}
