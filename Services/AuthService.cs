namespace ServerChat.Services
{
    public class AuthService
    {
        private DatabaseService _db;

        public AuthService()
        {
            _db = new DatabaseService();
        }

        public string Register(string username, string password)
        {
            if (_db.UserExists(username))
                return "USER_EXISTS";

            string hash = EncryptionService.HashPassword(password);
            _db.RegisterUser(username, hash);

            return "REGISTER_SUCCESS";
        }

        public string Login(string username, string password)
        {
            string hash = EncryptionService.HashPassword(password);

            if (_db.ValidateUser(username, hash))
                return "LOGIN_SUCCESS";

            return "LOGIN_FAILED";
        }
    }
}