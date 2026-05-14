using System.Text.Json;
using System.IO;
namespace MessageApp
{
    public class UserSettings
    {
        private static UserSettings _instance;
        public static UserSettings Instance => _instance ??= Load();

        public string Nickname { get; set; } = "";
        public DateTime? BirthDate { get; set; }
        public string AvatarPath { get; set; } = "";
        public bool DarkTheme { get; set; } = true;
        public int RefreshInterval { get; set; } = 2;

        private static string SettingsPath => Path.Combine(Environment.CurrentDirectory, "user_settings.json");

        private static UserSettings Load()
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
            }
            return new UserSettings();
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(SettingsPath, json);
        }
    }
}
