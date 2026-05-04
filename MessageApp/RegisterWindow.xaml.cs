using System.Windows;
using System.Windows.Controls;



namespace MessageApp
{
    public partial class RegisterWindow : Window
    {
        private AuthService _auth = AuthService.Instance;

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (_auth.Register(LoginBox.Text, PasswordBox.Password, out string error))
            {
                MessageBox.Show("Ok");
                this.Close();
            }
            else
            {
                ErrorText.Text = error;
            }
        }
    }
}
