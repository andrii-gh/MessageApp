using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MessageApp
{
    /// <summary>
    /// Interaction logic for ChatsWindow.xaml
    /// </summary>
    public partial class ChatsWindow : Window
    {
        private ChatService service = new ChatService();

        public ChatsWindow()
        {
            InitializeComponent();

            LoadChats();
        }

        private async void LoadChats()
        {
            ChatsList.ItemsSource = await service.GetChats();
        }

        private async void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            await service.CreateChat(ChatNameBox.Text);

            LoadChats();
        }
    }
}
