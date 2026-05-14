using System.Buffers.Text;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MessageApp
{
    public class HttpChatService
    {
        HttpClient client = new() { Timeout = TimeSpan.FromSeconds(10) };
        string baseUrl = "http://localhost:5000/api/";

        public async Task<List<Chat>> GetChats()
        {
            try
            {
                var json = await client.GetStringAsync(baseUrl + "chats");
                return JsonSerializer.Deserialize<List<Chat>>(json) ?? new();
            }
            catch { return new(); }
        }

        public async Task CreateChat(string name)
        {
            var content = new StringContent(JsonSerializer.Serialize(name), Encoding.UTF8, "application/json");
            await client.PostAsync(baseUrl + "chats", content);
        }

        public async Task<List<Message>> GetMessages(int chatId)
        {
            try
            {
                var json = await client.GetStringAsync(baseUrl + $"messages?chatId={chatId}");
                return JsonSerializer.Deserialize<List<Message>>(json) ?? new();
            }
            catch { return new(); }
        }

        public async Task SendMessage(Message msg)
        {
            var content = new StringContent(JsonSerializer.Serialize(msg), Encoding.UTF8, "application/json");
            await client.PostAsync(baseUrl + "messages", content);
        }
        public async Task<UserProfile?> GetUserProfile(string login)
        {
            try
            {
                var json = await client.GetStringAsync($"{baseUrl}user/{login}");
                return JsonSerializer.Deserialize<UserProfile>(json);
            }
            catch
            {
                return null;
            }
        }

        public async Task UpdateUserProfile(string login, UserProfile profile)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { profile.Nickname, profile.AvatarPath, profile.BirthDate }),
                Encoding.UTF8,
                "application/json");
            await client.PostAsync($"{baseUrl}user/{login}", content);
        }

        public class UserProfile
        {
            public string Nickname { get; set; } = "";
            public string AvatarPath { get; set; } = "";
            public DateTime? BirthDate { get; set; }
        }
    }
}