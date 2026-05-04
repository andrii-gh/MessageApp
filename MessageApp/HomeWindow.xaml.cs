using System.Windows;

namespace MessageApp
{
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();

            UserText.Text = $"Hello, {Session.CurrentUser}";
        }
    }
}
