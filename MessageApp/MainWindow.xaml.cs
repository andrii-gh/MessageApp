using System.Windows;

namespace MessageApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_button_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Login button clicked!");
            }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Доброго вечора Ми з України");
        }
    }
}