using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Babble.Core.Objects;

namespace Server.Dal
{
    class ChannelDal
    {
        public List<Channel> GetAllChannels()
        {
            const string sql = @"
select * from Channels
";
            using (var conn = Database.CreateConnection())
            {
                var channels = conn.Query<Channel>(sql).ToList();
                return channels;
            }
        }

        public Channel GetChannel(int id)
        {
            const string sql = @"
select * from Channels
where Id = @id
";
            using (var conn = Database.CreateConnection())
            {
                var channel = conn.Query<Channel>(sql, new { id = id}).FirstOrDefault();
                return channel;
            }
        }

        public int CreateChannel(string name)
        {
            const string sql = @"
insert into Channels (Name) values (@name);
select last_insert_rowid() from Channels;
";
            using (var conn = Database.CreateConnection())
            {
                var createdChannelId = conn.Query<int>(sql, new { name = name }).First();
                return createdChannelId;
            }
        }

        public void UpdateChannel(Channel channel)
        {
            const string sql = @"
update Channels
set Name = @name
where Id = @id;
";
            using (var conn = Database.CreateConnection())
            {
                conn.Execute(sql, new { name = channel.Name, id = channel.Id });
            }
        }

        public void DeleteChannel(int id)
        {
            const string sql = @"
delete from Channels
where Id = @id;
";
            using (var conn = Database.CreateConnection())
            {
                conn.Execute(sql, new { id = id });
            }
        }
    }
}
