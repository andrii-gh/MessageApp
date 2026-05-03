using System.Windows;



namespace MessageApp
{
    public partial class MainWindow : Window
    {
        private AuthService _auth = AuthService.Instance;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (_auth.Login(LoginBox.Text, PasswordBox.Password, out string error))
            {
                Session.CurrentUser = LoginBox.Text;

                new HomeWindow().Show();
                this.Close();
            }
            else
            {
                ErrorText.Text = error;
            }
        }

        private void OpenRegister_Click(object sender, RoutedEventArgs e)
        {
            new RegisterWindow().Show();
        }
    }


}
