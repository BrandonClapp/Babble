using Babble.Core.Objects;
using Server.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    class UserService
    {
        public UserDal userDal = new UserDal();

        public List<UserInfo> GetAllUsers()
        {
            var users = userDal.GetAllUsers();
            return users;
        }

        public UserInfo GetUser(int id)
        {
            var user = userDal.GetUser(id);
            return user;
        }

        public void UpdateUser(UserInfo user)
        {
            userDal.UpdateUser(user);
        }

        public void DeleteUser(int id)
        {
            userDal.DeleteUser(id);
        }

        public bool IsUserAuthenticated(string userName, string password)
        {
            // TODO, apply custom logic such as hashing
            // for now just send straight password to database
            var result = userDal.IsUserAuthenticated(userName, password);
            return result;
        }
    }
}
