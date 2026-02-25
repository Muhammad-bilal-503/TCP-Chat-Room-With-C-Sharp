using System.Collections.Generic;

namespace ServerChat.Services
{
    public class AuthService
    {
        private static Dictionary<string, string> _users = new Dictionary<string, string>();

        public bool Register(string username, string password)
        {
            if (_users.ContainsKey(username))
                return false;

            _users.Add(username, password);
            return true;
        }

        public bool Login(string username, string password)
        {
            if (_users.ContainsKey(username) && _users[username] == password)
                return true;

            return false;
        }
    }
}