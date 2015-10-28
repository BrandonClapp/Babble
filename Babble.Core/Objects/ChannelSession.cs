using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core.Objects
{
    public class ChannelSession
    {
        public ChannelSession(Channel channel)
        {
            Channel = channel;
        }

        public Channel Channel { get; private set; }
        public List<UserSession> UserSessions { get; private set; } = new List<UserSession>();

        public void AddUser(UserSession userSession)
        {
            // validation logic for adding user here
            UserSessions.Add(userSession);
            userSession.ChannelId = Channel.Id;
        }

        public void RemoveUser(UserSession userSession)
        {
            UserSessions.Remove(userSession);
            userSession.ChannelId = -1;
        }
    }
}
