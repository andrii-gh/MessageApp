using System.Windows;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MessageApp
{
    public partial class HomeWindow : Window
    {
        private bool isLoading = false;
        HttpChatService service = new();
        private int currentChatId = 0;
        private DispatcherTimer timer = new();
        private string currentUser = Session.CurrentUser ?? "Unknown";
        private List<Chat> allChats = new();
        private List<Message> currentMessages = new();

        public HomeWindow()
        {
            InitializeComponent();

            LoginText.Text = $"Login: {currentUser}";

            LoadChats();
            _ = LoadUserProfile();

            timer.Interval = TimeSpan.FromSeconds(2);

            timer.Tick += async (s, e) =>
            {
                if (currentChatId == 0 || isLoading)
                    return;

                try
                {
                    isLoading = true;
                    await LoadMessages();
                }
                finally
                {
                    isLoading = false;
                }
            };

        }

        private void OpenTaskManager_Click(object sender, RoutedEventArgs e)
        {
            if (currentChatId == 0)
            {
                MessageBox.Show("Please select a chat first", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var todoWindow = new TodoWindow(currentChatId);
            todoWindow.Owner = this;
            todoWindow.ShowDialog();
        }
        private async void LoadChats()
        {
            allChats = await service.GetChats();

            ChatsList.ItemsSource = allChats;
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
                MessageBox.Show(ex.Message);
            }
        }

        private async void ChatsList_SelectionChanged(object sender,
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ChatsList.SelectedItem is Chat chat)
            {
                currentChatId = chat.Id;

                await LoadMessages();
            }
        }

        private async Task LoadMessages()
        {
            LoadingText.Visibility = Visibility.Visible;

            currentMessages = await service.GetMessages(currentChatId);

            ShowMessages(currentMessages);

            LoadingText.Visibility = Visibility.Collapsed;
        }

        private void ShowMessages(List<Message> msgs)
        {
            ChatBox.Clear();

            ChatBox.Text = string.Join("\n", msgs);

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

            Sidebar.Visibility = menuOpen
                ? Visibility.Collapsed
                : Visibility.Visible;

            SidebarColumn.Width = menuOpen
                ? new GridLength(0)
                : new GridLength(200);

            OpenMenuButton.Visibility = menuOpen
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.CurrentUser = null;

            new MainWindow().Show();

            Close();
        }
        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow(this);
            settings.Owner = this;
            settings.ShowDialog();
        }

        public void UpdateUserInfo()
        {
            if (!string.IsNullOrEmpty(UserSettings.Instance.AvatarPath) && File.Exists(UserSettings.Instance.AvatarPath))
            {
                AvatarImage.Source = new BitmapImage(new Uri(UserSettings.Instance.AvatarPath));
            }

            string displayName = string.IsNullOrEmpty(UserSettings.Instance.Nickname)
                ? Session.CurrentUser
                : UserSettings.Instance.Nickname;
            Title = $"Messenger - {displayName}";
        }

        private void MsgBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                Send_Click(sender, e);
                e.Handled = true;
            }
        }


        private async void SearchMessageBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string text = SearchMessageBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(text))
            {
                await LoadMessages();
                return;
            }

            if (currentChatId != 0)
            {
                var allMessages = await service.GetMessages(currentChatId);
                var filtered = allMessages
                    .Where(m => m.Text.ToLower().Contains(text) ||
                                m.Username.ToLower().Contains(text))
                    .ToList();
                ShowMessages(filtered);
            }
            else
            {
                ChatBox.Clear();
                ChatBox.AppendText("Select a chat first to search messages\n");
            }
        }
        private void SearchChatBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string text = SearchChatBox.Text.ToLower();

            var filtered = allChats
                .Where(c => c.Name.ToLower().Contains(text))
                .ToList();

            ChatsList.ItemsSource = filtered;
        }

        private async Task LoadUserProfile()
        {
            var profile = await service.GetUserProfile(currentUser);
            if (profile != null)
            {
                UserSettings.Instance.Nickname = profile.Nickname;
                UserSettings.Instance.BirthDate = profile.BirthDate;
                UserSettings.Instance.AvatarPath = profile.AvatarPath;
                UpdateUserInfo();
            }
        }
    }
}