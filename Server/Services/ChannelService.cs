using Babble.Core.Objects;
using Server.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    class ChannelService
    {
        private ChannelDal channelDal = new ChannelDal();

        public List<Channel> GetAllChannels()
        {
            var channels = channelDal.GetAllChannels();
            return channels;
        }

        public Channel GetChannel(int id)
        {
            var channel = channelDal.GetChannel(id);
            return channel;
        }

        public Channel CreateChannel(string name)
        {
            var createdChannelId = channelDal.CreateChannel(name);
            return new Channel()
            {
                Id = createdChannelId,
                Name = name
            };
        }

        public void UpdateChannel(Channel channel)
        {
            channelDal.UpdateChannel(channel);
        }

        public void DeleteChannel(int id)
        {
            channelDal.DeleteChannel(id);
        }
    }
}
