using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Windows.Interop;

namespace MessageApp
{
    public class LocalServer
    {
        private HttpListener listener = null!;
        private List<Message> messages = new();
        private List<Chat> chats = new();
        private int nextChatId = 1;
        private int nextMessageId = 1;
        private readonly object _lock = new();

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

                if (request.HttpMethod == "GET" && path == "/api/chats")
                {
                    lock (_lock)
                    {
                        var json = JsonSerializer.Serialize(chats);
                        var buffer = Encoding.UTF8.GetBytes(json);
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                }
                else if (request.HttpMethod == "GET" && path.StartsWith("/api/messages"))
                {
                    var query = System.Web.HttpUtility.ParseQueryString(request.Url.Query);
                    if (int.TryParse(query["chatId"], out int chatId))
                    {
                        lock (_lock)
                        {
                            var msgs = messages.Where(x => x.ChatId == chatId).ToList();
                            var json = JsonSerializer.Serialize(msgs);
                            var buffer = Encoding.UTF8.GetBytes(json);
                            response.OutputStream.Write(buffer, 0, buffer.Length);
                        }
                    }
                }
                else if (request.HttpMethod == "GET" && path.StartsWith("/api/user/"))
                {
                    var login = path.Split('/').Last();
                    var user = AuthService.Instance.GetUser(login);

                    if (user != null)
                    {
                        var profile = new { user.Nickname, user.AvatarPath, user.BirthDate };
                        var json = JsonSerializer.Serialize(profile);
                        var buffer = Encoding.UTF8.GetBytes(json);
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        response.StatusCode = 404;
                    }
                }
                else if (request.HttpMethod == "POST" && path == "/api/chats")
                {

                    using var reader = new StreamReader(request.InputStream);
                    var body = await reader.ReadToEndAsync();
                    var name = JsonSerializer.Deserialize<string>(body);

                    lock (_lock)
                    {
                        chats.Add(new Chat { Id = nextChatId++, Name = name ?? "Chat" });
                    }
                    response.StatusCode = 200;
                }
                else if (request.HttpMethod == "POST" && path == "/api/messages")
                {
                    using var reader = new StreamReader(request.InputStream);
                    var body = await reader.ReadToEndAsync();
                    var msg = JsonSerializer.Deserialize<Message>(body);
                    if (msg != null)
                    {
                        lock (_lock)
                        {
                            msg.Id = nextMessageId++;
                            msg.Timestamp = DateTime.Now;

                            messages.Add(msg);

                            var botReply = GetBotReply(msg.Text);

                            if (!string.IsNullOrEmpty(botReply))
                            {
                                var botMessage = new Message
                                {
                                    ChatId = msg.ChatId,
                                    Username = "BOT",
                                    Text = botReply,
                                    Timestamp = DateTime.Now
                                };

                                botMessage.Id = nextMessageId++;

                                messages.Add(botMessage);
                            }
                        }
                    }

                    response.StatusCode = 200;
                }
                else if (request.HttpMethod == "POST" && path.StartsWith("/api/user/"))
                {
                    var login = path.Split('/').Last();
                    using var reader = new StreamReader(request.InputStream);
                    var body = await reader.ReadToEndAsync();
                    var updates = JsonSerializer.Deserialize<Dictionary<string, object>>(body);

                    if (AuthService.Instance.UpdateUser(login, updates))
                        response.StatusCode = 200;
                    else
                        response.StatusCode = 404;
                }


                response.Close();
               
            }
        }

                private string GetBotReply(string userMessage)
        {
            string msg = userMessage.ToLower();

            if (msg.Contains("hello") || msg.Contains("hi") || msg.Contains("Hi"))
                return "Hello how are you";

            if (msg.Contains("how are you") || msg.Contains("you?"))
                return "i am good, what about you?";

            if (msg.Contains("help") || msg.Contains("please help"))
                return "what kind of problem you have?.";

            if (msg.Contains("thank you") || msg.Contains("thanks"))
                return "Always happy to help!";

            if (msg.Contains("bot") || msg.Contains("are you a bot"))
                return "No, I'm a gamer stuck in the matrix but I guess I'm a bot...";

            return "";
        }
    }
        }
