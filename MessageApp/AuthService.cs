namespace MessageApp
{
    public class AuthService
    {
        private static AuthService? _instance;
        public static AuthService Instance => _instance ??= new AuthService();


        private List<User> _users = new()
        {
            new User
            {
                Login = "Sasha",
                Password = "1234"
            },

            new User
            {
                Login = "Andriy",
                Password = "1234"
            },
                new User
                {
                    Login = "Artem",
                    Password = "1234"
                }


        };

        public bool Register(string login, string password, out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                error = "Fields are empty";
                return false;
            }

            if (_users.Any(u => u.Login == login))
            {
                error = "User already exists";
                return false;
            }

            _users.Add(new User { Login = login, Password = password });
            return true;
        }


        public bool Login(string login, string password, out string error)
        {
            error = "";

            var user = _users.FirstOrDefault(u => u.Login == login && u.Password == password);

            if (user == null)
            {
                error = "Wrong login or password";
                return false;
            }

            return true;

        }
    }
}
