using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace MessageApp
{
    public partial class HomeWindow : Window
    {
        LocalServer server = new();
        HttpChatService service = new();

        bool menuOpen = true;
        int currentChatId = 0;
        DispatcherTimer timer = new();

        public HomeWindow()
        {
            InitializeComponent();

            server.Start();

            LoginText.Text = $"Login: {Session.CurrentUser}";
            LoadChats();

            timer.Interval =
                TimeSpan.FromSeconds(2);

            timer.Tick += async (s, e) =>
            {
                if (currentChatId != 0)
                    await LoadMessages();
            };

            timer.Start();
        }

        private async void LoadChats()
        {
            ChatsList.ItemsSource =
                await service.GetChats();
        }

        private async void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            string name =
                "Chat " + DateTime.Now.Second;

            await service.CreateChat(name);

            LoadChats();
        }

        private async void ChatsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ChatsList.SelectedItem is Chat chat)
            {
                currentChatId = chat.Id;

                await LoadMessages();
            }
        }

        private async Task LoadMessages()
        {
            var msgs =
                await service.GetMessages(currentChatId);

            ChatBox.Clear();

            foreach (var msg in msgs)
            {
                ChatBox.AppendText(msg.Text + "\n");
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MsgBox.Text))
            {
                MessageBox.Show("Enter message");
                return;
            }

            string msg = $"{Session.CurrentUser}: {MsgBox.Text}";

            await service.Send(msg);

            ChatBox.AppendText(msg + "\n");

            MsgBox.Clear();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            menuOpen = !menuOpen;

            if (menuOpen)
            {
                SidebarColumn.Width = new GridLength(200);

                Sidebar.Visibility = Visibility.Visible;

                OpenMenuButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SidebarColumn.Width = new GridLength(0);

                Sidebar.Visibility = Visibility.Collapsed;

                OpenMenuButton.Visibility = Visibility.Visible;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.CurrentUser = null;

            new MainWindow().Show();

            Close();
        }
    }
}
