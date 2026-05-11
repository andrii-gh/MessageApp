using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageApp
{
    public class MessageService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string BaseUrl = "http://localhost:5000/api/chat/";

        public async Task<List<Message>> GetMessagesAsync(string username, DateTime since)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/messages?user={username}&since={since:O}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Message>>(json) ?? new List<Message>();
            }
            catch (Exception ex)
            {
                return new List<Message>();
            }
        }

        public async Task<bool> SendMessageAsync(string username, string text)
        {
            try
            {
                var message = new { Username = username, Text = text, Timestamp = DateTime.UtcNow };
                var json = JsonSerializer.Serialize(message);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/messages", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    public class Message
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
