using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MessageApp
{
    public class LocalServer
    {
        private HttpListener listener = null!;
        private List<string> messages = new();

        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/api/chat/");
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

                if (request.HttpMethod == "GET")
                {
                    var json = JsonSerializer.Serialize(messages);
                    var buffer = Encoding.UTF8.GetBytes(json);

                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else if (request.HttpMethod == "POST")
                {
                    using var reader = new StreamReader(request.InputStream);
                    var body = await reader.ReadToEndAsync();

                    var msg = JsonSerializer.Deserialize<string>(body);

                    if (!string.IsNullOrWhiteSpace(msg))
                        messages.Add(msg);

                    response.StatusCode = 200;
                }

                response.Close();
            }
        }
    }
}
