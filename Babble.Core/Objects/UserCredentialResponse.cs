using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core.Objects
{
    public class UserCredentialResponse
    {
        public bool IsAuthenticated { get; set; }
        public string Message { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
