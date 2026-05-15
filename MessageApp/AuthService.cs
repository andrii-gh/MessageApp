using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageApp
{
    public class AuthService
    {
        private static AuthService? _instance;
        public static AuthService Instance => _instance ??= new AuthService();

        private AuthService() { }

        public bool Register(string login, string password, out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                error = "Fields are empty";
                return false;
            }

            var result = FileDatabase.Instance.RegisterUser(login, password).Result;
            if (!result)
            {
                error = "User already exists";
                return false;
            }

            return true;
        }

        public bool Login(string login, string password, out string error)
        {
            error = "";

            var isValid = FileDatabase.Instance.ValidateUser(login, password).Result;
            if (!isValid)
            {
                error = "Wrong login or password";
                return false;
            }

            return true;
        }

        public FileDatabase.UserData? GetUser(string login)
        {
            return FileDatabase.Instance.GetUser(login).Result;
        }

        public bool UpdateUser(string login, Dictionary<string, object> updates)
        {
            string nickname = "";
            string avatarPath = "";
            DateTime? birthDate = null;

            if (updates.TryGetValue("nickname", out var nick))
                nickname = nick?.ToString() ?? "";
            if (updates.TryGetValue("avatarPath", out var avatar))
                avatarPath = avatar?.ToString() ?? "";
            if (updates.TryGetValue("birthDate", out var birth) && birth != null)
                birthDate = DateTime.Parse(birth.ToString());

            return FileDatabase.Instance.UpdateUser(login, nickname, avatarPath, birthDate).Result;
        }
    }
}