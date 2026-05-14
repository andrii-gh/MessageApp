using System.Windows;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MessageApp
{
    public partial class HomeWindow : Window
    {
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
                if (currentChatId != 0)
                    await LoadMessages();
            };

            timer.Start();

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
            currentMessages = await service.GetMessages(currentChatId);

            ShowMessages(currentMessages);
        }

        private void ShowMessages(List<Message> msgs)
        {
            ChatBox.Clear();

            foreach (var msg in msgs)
            {
                ChatBox.AppendText(msg + "\n");
            }

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