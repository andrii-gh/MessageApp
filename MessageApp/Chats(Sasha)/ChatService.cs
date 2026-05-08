using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MessageApp
{
    public class ChatService
    {
        HttpClient client = new();

        string url = "http://localhost:5000/api/chats/";

        public async Task<List<Chat>> GetChats()
        {
            var res = await client.GetAsync(url);

            var json = await res.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<Chat>>(json) ?? new();
        }

        public async Task CreateChat(string name)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(name),
                Encoding.UTF8,
                "application/json");

            await client.PostAsync(url, content);
        }
    }
}
