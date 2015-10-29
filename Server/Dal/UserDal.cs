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
        const string SelectUserSafeData = @"
select
    Id
    ,Username
    ,UserType
from Users
";

        public List<UserInfo> GetAllUsers()
        {
            string sql = $@"
{SelectUserSafeData}
";
            using (var conn = Database.CreateConnection())
            {
                var users = conn.Query<UserInfo>(sql).ToList();
                return users;
            }
        }

        public UserInfo GetUser(int id)
        {
            string sql = $@"
{SelectUserSafeData}
where Id = @id
";
            using (var conn = Database.CreateConnection())
            {
                var user = conn.Query<UserInfo>(sql, new { id = id }).FirstOrDefault();
                return user;
            }
        }

        public UserInfo GetUserByUsername(string username)
        {
            string sql = $@"
{SelectUserSafeData}
where Username = @username
";
            using (var conn = Database.CreateConnection())
            {
                var user = conn.Query<UserInfo>(sql, new { username = username }).FirstOrDefault();
                return user;
            }
        }

        public UserInfo GetUserCredential(string username)
        {
            const string sql = @"
select UserName, Password, Salt
from Users
where Username = @username
";
            using (var conn = Database.CreateConnection())
            {
                var user = conn.Query<UserInfo>(sql, new { username = username }).FirstOrDefault();
                return user;
            }
        }

        public int CreateUser(UserInfo userInfo)
        {
            const string sql = @"
insert into Users 
(Username, Password, Salt, UserType) 
values (@username, @password, @salt, @userType);
select last_insert_rowid() from Users;
";
            using (var conn = Database.CreateConnection())
            {
                var createdChannelId = conn.Query<int>(sql, 
                    new
                    {
                        username = userInfo.Username,
                        password = userInfo.Password,
                        salt = userInfo.Salt,
                        userType = userInfo.UserType
                    }).First();
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
    }
}
