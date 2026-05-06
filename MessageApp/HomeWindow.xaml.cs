using System.Windows;

namespace MessageApp
{
    public partial class HomeWindow : Window
    {
        LocalServer server = new();
        HttpChatService service = new(); 

        public HomeWindow()
        {
            InitializeComponent();
   
            server.Start();
            StartPolling();
            
        }


        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MsgBox.Text))
            {
                MessageBox.Show("Empty message");
                return;
            }


                MessageBox.Show("Network error");

            MsgBox.Clear();
        }

        async Task LoadMessages()
        {
            var msgs = await service.GetMessages();

            ChatBox.Text = string.Join("\n", msgs ?? new List<string>());
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
}//пп