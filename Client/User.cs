using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class User
    {
        public string Username { get; set; }
        private string passwordHash;

        public string Password
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
