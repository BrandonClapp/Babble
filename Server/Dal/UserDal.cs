using Babble.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace Server.Dal
{
    class UserDal
    {
        public List<UserInfo> GetAllUsers()
        {
            const string sql = @"
select Id, UserName 
from Users
";
            using (var conn = Database.CreateConnection())
            {
                var users = conn.Query<UserInfo>(sql).ToList();
                return users;
            }
        }

        public UserInfo GetUser(int id)
        {
            const string sql = @"
select Id, UserName
from Users
where Id = @id
";
            using (var conn = Database.CreateConnection())
            {
                var channel = conn.Query<UserInfo>(sql, new { id = id }).FirstOrDefault();
                return channel;
            }
        }

        public int CreateUser(string userName, string password)
        {
            const string sql = @"
insert into Users (UserName, Password) values (@name, @password);
select last_insert_rowid() from Users;
";
            using (var conn = Database.CreateConnection())
            {
                var createdChannelId = conn.Query<int>(sql, 
                    new { userName = userName, password = password}).First();
                return createdChannelId;
            }
        }

        public void UpdateUser(UserInfo user)
        {
            // TODO: need to find out what can you update
            // update password should be a separate operation
        }

        public void DeleteUser(int id)
        {
            const string sql = @"
delete from Users
where Id = @id;
";
            using (var conn = Database.CreateConnection())
            {
                conn.Execute(sql, new { id = id });
            }
        }

        public bool IsUserAuthenticated(string userName, string password)
        {
            const string sql = @"
select top 1 *
from Users
where UserName = @userName and Password = @password
";
            using (var conn = Database.CreateConnection())
            {
                var result = conn.Query<int>(sql, new { userName = userName, password = password }).First();
                return result > 0 ? true : false;
            }
        }
    }
}
