namespace MessageApp
{
    public class User
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string Nickname { get; set; } = "";
        public string AvatarPath { get; set; } = "";
        public DateTime? BirthDate { get; set; }
    }
}
