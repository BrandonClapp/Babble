using Babble.Core.Objects;
using Server.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    class DatabaseService
    {
        UserService userService = new UserService();
        /// <summary>
        /// If database already existed, do nothing
        /// If not, create a new default one
        /// </summary>
        public void InitDatabase()
        {
            if (Database.Exists)
            {
                return;
            }

            Database.CreateDefaultDatabase();
            userService.CreateUser("admin", "123", UserType.Admin);
        }
    }
}
