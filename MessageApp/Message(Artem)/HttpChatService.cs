using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MessageApp
{
    public class HttpChatService
    {
        HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(3)
        };

        string url = "http://localhost:8080/api/chat/";

        public async Task<List<string>> GetMessages()
        {
            try
            {
                var res = await client.GetAsync(url);
                var json = await res.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<string>>(json) ?? new();
            }
            catch
            {
                return new();
            }
        }

        public async Task Send(string msg)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(msg),
                Encoding.UTF8,
                "application/json");

            await client.PostAsync(url, content);
        }
    }
}
