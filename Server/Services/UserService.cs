using Babble.Core.Objects;
using Server.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    class UserService
    {
        const int SaltLength = 32;

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

        public UserInfo GetUserByUsername(string username)
        {
            var user = userDal.GetUserByUsername(username);
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

        public bool IsUserAuthenticated(string username, string password)
        {
            var userCredential = userDal.GetUserCredential(username);
            if (userCredential == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            using (var shazam = new SHA512Managed())
            {
                var passwordAndSalt = password + userCredential.Salt;
                var hash = Encoding.UTF8.GetString(shazam.ComputeHash(Encoding.UTF8.GetBytes(passwordAndSalt)));
                var authenticated = hash.Equals(userCredential.Password);
                return authenticated;
            }
        }

        private string GenerateSaltValue()
        {
            byte[] rngBytes = new byte[SaltLength];
            using (var rng = RNGCryptoServiceProvider.Create())
            {
                rng.GetNonZeroBytes(rngBytes);
                return Encoding.UTF8.GetString(rngBytes);
            }
        }

        public void CreateUser(UserInfo userInfo)
        {
            if (userInfo == null)
            {
                throw new ArgumentNullException(nameof(userInfo));
            }
            if (string.IsNullOrWhiteSpace(userInfo.Username))
            {
                throw new ArgumentNullException(nameof(userInfo.Username));
            }

            if (string.IsNullOrWhiteSpace(userInfo.Password))
            {
                throw new ArgumentNullException(nameof(userInfo.Password));
            }

            if (UsernameExists(userInfo.Username))
            {
                throw new Exception("Username already exists, please enter a different username");
            }

            using (var shazam = new SHA512Managed())
            {
                var salt = GenerateSaltValue();
                var passwordAndSalt = userInfo.Password + salt;
                var hashedPassword = Encoding.UTF8.GetString(shazam.ComputeHash(Encoding.UTF8.GetBytes(passwordAndSalt)));
                userInfo.Password = hashedPassword;
                userInfo.Salt = salt;
                userDal.CreateUser(userInfo);
            }
        }

        private bool UsernameExists(string username)
        {
            var user = GetUserByUsername(username);
            return user != null;
        }
    }
}
