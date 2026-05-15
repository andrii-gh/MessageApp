using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MessageApp
{
    public class LocalServer
    {
        private HttpListener listener = null!;

        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();
            Task.Run(() => Listen());
        }

        private async Task Listen()
        {
            while (true)
            {
                var context = await listener.GetContextAsync();
                var request = context.Request;
                var response = context.Response;
                string path = request.Url.AbsolutePath;

                try
                {
                    // GET: /api/chats - отримати всі чати
                    if (request.HttpMethod == "GET" && path == "/api/chats")
                    {
                        var chats = await FileDatabase.Instance.GetChats();
                        var json = JsonSerializer.Serialize(chats.Select(c => new { c.Id, c.Name }));
                        SendResponse(response, json);
                    }
                    // GET: /api/messages?chatId=X - отримати повідомлення чату
                    else if (request.HttpMethod == "GET" && path.StartsWith("/api/messages"))
                    {
                        var query = System.Web.HttpUtility.ParseQueryString(request.Url.Query);
                        if (int.TryParse(query["chatId"], out int chatId))
                        {
                            var msgs = await FileDatabase.Instance.GetMessages(chatId);
                            var json = JsonSerializer.Serialize(msgs);
                            SendResponse(response, json);
                        }
                        else
                        {
                            response.StatusCode = 400;
                            response.Close();
                        }
                    }
                    // GET: /api/user/{login} - отримати профіль користувача
                    else if (request.HttpMethod == "GET" && path.StartsWith("/api/user/"))
                    {
                        var login = path.Split('/').Last();
                        var user = await FileDatabase.Instance.GetUser(login);

                        if (user != null)
                        {
                            var profile = new { user.Nickname, user.AvatarPath, user.BirthDate };
                            var json = JsonSerializer.Serialize(profile);
                            SendResponse(response, json);
                        }
                        else
                        {
                            response.StatusCode = 404;
                            response.Close();
                        }
                    }
                    // GET: /api/todos?chatId=X - отримати задачі чату
                    else if (request.HttpMethod == "GET" && path == "/api/todos")
                    {
                        var query = System.Web.HttpUtility.ParseQueryString(request.Url.Query);
                        if (int.TryParse(query["chatId"], out int chatId))
                        {
                            var items = await FileDatabase.Instance.GetTodos(chatId);
                            var json = JsonSerializer.Serialize(items);
                            SendResponse(response, json);
                        }
                        else
                        {
                            response.StatusCode = 400;
                            response.Close();
                        }
                    }
                    // POST: /api/chats - створити новий чат
                    else if (request.HttpMethod == "POST" && path == "/api/chats")
                    {
                        using var reader = new StreamReader(request.InputStream);
                        var body = await reader.ReadToEndAsync();
                        var name = JsonSerializer.Deserialize<string>(body);

                        var chat = await FileDatabase.Instance.CreateChat(name ?? "Chat", "");
                        response.StatusCode = 200;
                        response.Close();
                    }
                    // POST: /api/messages - відправити повідомлення
                    else if (request.HttpMethod == "POST" && path == "/api/messages")
                    {
                        using var reader = new StreamReader(request.InputStream);
                        var body = await reader.ReadToEndAsync();
                        var msg = JsonSerializer.Deserialize<Message>(body);

                        if (msg != null)
                        {
                            await FileDatabase.Instance.SaveMessage(msg.ChatId, msg.Username, msg.Text);

                            // Відповідь бота
                            var botReply = GetBotReply(msg.Text);
                            if (!string.IsNullOrEmpty(botReply))
                            {
                                await FileDatabase.Instance.SaveMessage(msg.ChatId, "BOT", botReply);
                            }
                        }
                        response.StatusCode = 200;
                        response.Close();
                    }
                    // POST: /api/user/{login} - оновити профіль користувача
                    else if (request.HttpMethod == "POST" && path.StartsWith("/api/user/"))
                    {
                        var login = path.Split('/').Last();
                        using var reader = new StreamReader(request.InputStream);
                        var body = await reader.ReadToEndAsync();
                        var updates = JsonSerializer.Deserialize<Dictionary<string, object>>(body);

                        if (updates != null)
                        {
                            string nickname = updates.TryGetValue("nickname", out var nick) ? nick?.ToString() ?? "" : "";
                            string avatarPath = updates.TryGetValue("avatarPath", out var avatar) ? avatar?.ToString() ?? "" : "";
                            DateTime? birthDate = null;

                            if (updates.TryGetValue("birthDate", out var birth) && birth != null)
                            {
                                birthDate = DateTime.Parse(birth.ToString());
                            }

                            var result = await FileDatabase.Instance.UpdateUser(login, nickname, avatarPath, birthDate);
                            response.StatusCode = result ? 200 : 404;
                        }
                        else
                        {
                            response.StatusCode = 400;
                        }
                        response.Close();
                    }
                    // POST: /api/todos - створити нову задачу
                    else if (request.HttpMethod == "POST" && path == "/api/todos")
                    {
                        using var reader = new StreamReader(request.InputStream);
                        var body = await reader.ReadToEndAsync();
                        var item = JsonSerializer.Deserialize<TodoItem>(body);

                        if (item != null)
                        {
                            await FileDatabase.Instance.CreateTodo(
                                item.ChatId,
                                item.Title,
                                item.Description,
                                item.CreatedBy,
                                item.DueDate
                            );
                        }
                        response.StatusCode = 200;
                        response.Close();
                    }
                    // PUT: /api/todos/{id} - оновити статус задачі
                    else if (request.HttpMethod == "PUT" && path.StartsWith("/api/todos/"))
                    {
                        var idStr = path.Split('/').Last();
                        if (int.TryParse(idStr, out int id))
                        {
                            using var reader = new StreamReader(request.InputStream);
                            var body = await reader.ReadToEndAsync();
                            var updates = JsonSerializer.Deserialize<Dictionary<string, object>>(body);

                            if (updates != null && updates.TryGetValue("isCompleted", out var isCompleted))
                            {
                                await FileDatabase.Instance.UpdateTodoStatus(id, Convert.ToBoolean(isCompleted));
                                response.StatusCode = 200;
                            }
                            else
                            {
                                response.StatusCode = 400;
                            }
                        }
                        else
                        {
                            response.StatusCode = 400;
                        }
                        response.Close();
                    }
                    // DELETE: /api/todos/{id} - видалити задачу
                    else if (request.HttpMethod == "DELETE" && path.StartsWith("/api/todos/"))
                    {
                        var idStr = path.Split('/').Last();
                        if (int.TryParse(idStr, out int id))
                        {
                            await FileDatabase.Instance.DeleteTodo(id);
                            response.StatusCode = 200;
                        }
                        else
                        {
                            response.StatusCode = 400;
                        }
                        response.Close();
                    }
                    else
                    {
                        response.StatusCode = 404;
                        response.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Server error: {ex.Message}");
                    response.StatusCode = 500;
                    response.Close();
                }
            }
        }

        private void SendResponse(HttpListenerResponse response, string json)
        {
            var buffer = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        private string GetBotReply(string userMessage)
        {
            string msg = userMessage.ToLower();

            if (msg.Contains("hello") || msg.Contains("hi"))
                return "Hello! How are you?";

            if (msg.Contains("how are you"))
                return "I am good, what about you?";

            if (msg.Contains("help"))
                return "How can I help you?";

            if (msg.Contains("thank you") || msg.Contains("thanks"))
                return "You're welcome!";

            if (msg.Contains("bot"))
                return "Yes, I'm a bot assistant!";

            return "";
        }
    }
}