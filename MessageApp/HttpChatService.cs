using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MessageApp
{
    public class HttpChatService
    {
        private HttpClient client = new() { Timeout = TimeSpan.FromSeconds(10) };
        private string baseUrl = "http://localhost:5000/api/";


        public async Task<List<Chat>> GetChats()
        {
            try
            {
                var json = await client.GetStringAsync(baseUrl + "chats");
                return JsonSerializer.Deserialize<List<Chat>>(json) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetChats error: {ex.Message}");
                return new();
            }
        }

        public async Task CreateChat(string name)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(name), Encoding.UTF8, "application/json");
                await client.PostAsync(baseUrl + "chats", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateChat error: {ex.Message}");
            }
        }


        public async Task<List<Message>> GetMessages(int chatId)
        {
            try
            {
                var json = await client.GetStringAsync(baseUrl + $"messages?chatId={chatId}");
                return JsonSerializer.Deserialize<List<Message>>(json) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMessages error: {ex.Message}");
                return new();
            }
        }

        public async Task SendMessage(Message msg)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(msg), Encoding.UTF8, "application/json");
                await client.PostAsync(baseUrl + "messages", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage error: {ex.Message}");
            }
        }

        public async Task<List<Message>> SearchMessages(int chatId, string searchText)
        {
            try
            {
                var allMessages = await GetMessages(chatId);
                return allMessages
                    .Where(m => m.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                                m.Username.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            catch
            {
                return new();
            }
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
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(new { profile.Nickname, profile.AvatarPath, profile.BirthDate }),
                    Encoding.UTF8,
                    "application/json");
                await client.PostAsync($"{baseUrl}user/{login}", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateUserProfile error: {ex.Message}");
            }
        }


        public async Task<List<TodoItem>> GetTodos(int chatId)
        {
            try
            {
                var json = await client.GetStringAsync(baseUrl + $"todos?chatId={chatId}");
                return JsonSerializer.Deserialize<List<TodoItem>>(json) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetTodos error: {ex.Message}");
                return new();
            }
        }

        public async Task<TodoItem> CreateTodo(TodoItem item)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(item), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(baseUrl + "todos", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TodoItem>(json) ?? item;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateTodo error: {ex.Message}");
            }
            return item;
        }

        public async Task<bool> UpdateTodoStatus(int todoId, bool isCompleted)
        {
            try
            {
                var updates = new Dictionary<string, object> { { "isCompleted", isCompleted } };
                var content = new StringContent(JsonSerializer.Serialize(updates), Encoding.UTF8, "application/json");
                var response = await client.PutAsync(baseUrl + $"todos/{todoId}", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTodo(int todoId, string title, string description, DateTime? dueDate)
        {
            try
            {
                var updates = new Dictionary<string, object>
                {
                    { "title", title },
                    { "description", description },
                    { "dueDate", dueDate }
                };
                var content = new StringContent(JsonSerializer.Serialize(updates), Encoding.UTF8, "application/json");
                var response = await client.PutAsync(baseUrl + $"todos/{todoId}", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteTodo(int todoId)
        {
            try
            {
                var response = await client.DeleteAsync(baseUrl + $"todos/{todoId}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> TestConnection()
        {
            try
            {
                var response = await client.GetAsync(baseUrl + "chats");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }


        public class UserProfile
        {
            public string Nickname { get; set; } = "";
            public string AvatarPath { get; set; } = "";
            public DateTime? BirthDate { get; set; }
        }
    }
}