using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MessageApp
{
    public class LocalServer
    {
        private HttpListener listener = null!;

        private List<Message> messages = new();

        private List<Chat> chats = new();

        private int nextChatId = 1;

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

                if (request.HttpMethod == "GET" &&
                    path.StartsWith("/api/chat"))
                {
                    var json =
                        JsonSerializer.Serialize(chats);

                    var buffer =
                        Encoding.UTF8.GetBytes(json);

                    response.OutputStream.Write(
                        buffer,
                        0,
                        buffer.Length);
                }
                else if (request.HttpMethod == "GET" &&
                         path.StartsWith("/api/messages/"))
                {
                    int chatId =
                        int.Parse(path.Split('/').Last());

                    var msgs = messages
                        .Where(x => x.ChatId == chatId)
                        .ToList();

                    var json =
                        JsonSerializer.Serialize(msgs);

                    var buffer =
                        Encoding.UTF8.GetBytes(json);

                    response.OutputStream.Write(
                        buffer,
                        0,
                        buffer.Length);
                }
                else if (request.HttpMethod == "POST" &&
                         path.StartsWith("/api/chat"))
                {
                    using var reader =
                        new StreamReader(request.InputStream);

                    var body =
                        await reader.ReadToEndAsync();

                    var name =
                        JsonSerializer.Deserialize<string>(body);

                    chats.Add(new Chat
                    {
                        Id = nextChatId++,
                        Name = name ?? "Chat"
                    });

                    response.StatusCode = 200;
                }
                else if (request.HttpMethod == "POST" &&
                         path.StartsWith("/api/message"))
                {
                    using var reader =
                        new StreamReader(request.InputStream);

                    var body =
                        await reader.ReadToEndAsync();

                    var msg =
                        JsonSerializer.Deserialize<Message>(body);

                    if (msg != null)
                    {
                        messages.Add(msg);

                        messages.Add(new Message
                        {
                            ChatId = msg.ChatId,
                            Text = "Bot: I received -> " + msg.Text
                        });
                    }

                    response.StatusCode = 200;
                }

                response.Close();
            }
        }
    }
}
