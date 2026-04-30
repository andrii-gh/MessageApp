using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace MessageApp
{

        public partial class MainWindow : Window
        {
            HttpClient client = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(3)
            };

            string url = "https://localhost:5001/api/chat";

            public MainWindow()
            {
                InitializeComponent();
                StartPolling();
            }

            async Task LoadMessages()
            {
                try
                {
                    var res = await client.GetAsync(url);

                    if (!res.IsSuccessStatusCode)
                    {
                        chatBox.Text = "Server error";
                        return;
                    }

                    var json = await res.Content.ReadAsStringAsync();
                    var msgs = JsonSerializer.Deserialize<List<string>>(json);

                    chatBox.Text = string.Join("\n", msgs!);
                }
                catch (HttpRequestException)
                {
                    chatBox.Text = "Сервер недоступний";
                }
                catch (TaskCanceledException)
                {
                    chatBox.Text = "Timeout";
                }
            }

            private async void Send_Click(object sender, RoutedEventArgs e)
            {
                if (string.IsNullOrWhiteSpace(msgTb.Text))
                {
                    MessageBox.Show("Пусте повідомлення!");
                    return;
                }

                try
                {
                    var content = new StringContent(
                        JsonSerializer.Serialize(msgTb.Text),
                        Encoding.UTF8,
                        "application/json");

                    var res = await client.PostAsync(url, content);

                    if (!res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Помилка відправки");
                        return;
                    }

                    msgTb.Clear();
                }
                catch
                {
                    MessageBox.Show("Помилка мережі");
                }
            }

            async void StartPolling()
            {
                while (true)
                {
                    await LoadMessages();
                    await Task.Delay(2000);
                }
            }
        }
    }

    