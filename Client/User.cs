using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Client
{
    class MyMainWindow
    {
        public MyMainWindow()
        {
            User u = new User();
            u.Username = "Brandon";
            u.PasswordHash = "asdfasfd";
        }
        
        
    }

    class User
    {
        public string Username { get; set; }
        private string passwordHash;

        public string PasswordHash
        {
            get
            {
                return this.passwordHash;
            }
            set
            {
                this.passwordHash = UTF8Encoding.UTF8.GetString(SHA1.Create().ComputeHash(UTF8Encoding.UTF8.GetBytes(value)));
            }
        }


    }
}
