using System.Windows;
using System.Windows.Threading;

namespace MessageApp
{

    public partial class HomeWindow : Window
    {
        HttpChatService service = new();
        private int currentChatId = 0;
        private DispatcherTimer timer = new();
        private string currentUser = Session.CurrentUser ?? "Unknown";

        public HomeWindow()
        {
            InitializeComponent();
            LoginText.Text = $"Login: {currentUser}";
            LoadChats();

            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += async (s, e) =>
            {
                if (currentChatId != 0)
                    await LoadMessages();
            };
            timer.Start();
        }


        private async void LoadChats()
        {
            ChatsList.ItemsSource = await service.GetChats();
        }

        private async void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = "Chat " + DateTime.Now.Second;
                await service.CreateChat(name);
                LoadChats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error of creating chat: {ex.Message}");
            }

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
            var msgs = await service.GetMessages(currentChatId);
            ChatBox.Clear();
            foreach (var msg in msgs)
                ChatBox.AppendText(msg.ToString() + "\n");
            ChatBox.ScrollToEnd();
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MsgBox.Text))
            {
                MessageBox.Show("Enter message");
                return;
            }

            var msg = new Message
            {
                ChatId = currentChatId,
                Username = currentUser,
                Text = MsgBox.Text
            };

            await service.SendMessage(msg);
            MsgBox.Clear();
            await LoadMessages();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            bool menuOpen = Sidebar.Visibility == Visibility.Visible;
            Sidebar.Visibility = menuOpen ? Visibility.Collapsed : Visibility.Visible;
            SidebarColumn.Width = menuOpen ? new GridLength(0) : new GridLength(200);
            OpenMenuButton.Visibility = menuOpen ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.CurrentUser = null;
            new MainWindow().Show();
            Close();
        }
    }
}
