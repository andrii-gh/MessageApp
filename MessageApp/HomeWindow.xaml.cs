using System.Windows;

namespace MessageApp
{
    public partial class HomeWindow : Window
    {
        LocalServer server = new();
        HttpChatService service = new();

        bool menuOpen = true;

        public HomeWindow()
        {
            InitializeComponent();

            server.Start();

            LoginText.Text = $"Login: {Session.CurrentUser}";
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