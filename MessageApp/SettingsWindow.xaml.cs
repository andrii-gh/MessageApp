using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MessageApp
{
    public partial class SettingsWindow : Window
    {
        private string _avatarPath = "";
        private HomeWindow _parent;
        private HttpChatService service = new();

        public SettingsWindow(HomeWindow parent)
        {
            InitializeComponent();
            _parent = parent;
            LoadUserData();
        }

        private void LoadUserData()
        {
            NicknameBox.Text = UserSettings.Instance.Nickname;
            BirthDatePicker.SelectedDate = UserSettings.Instance.BirthDate;

            if (!string.IsNullOrEmpty(UserSettings.Instance.AvatarPath) && File.Exists(UserSettings.Instance.AvatarPath))
            {
                ProfileAvatar.Source = new BitmapImage(new Uri(UserSettings.Instance.AvatarPath));
            }
        }

        private void ChooseAvatar_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp";
            dialog.Title = "Select an avatar image";

            if (dialog.ShowDialog() == true)
            {
                string destPath = System.IO.Path.Combine(Environment.CurrentDirectory, "Avatars", System.IO.Path.GetFileName(dialog.FileName));
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destPath));
                File.Copy(dialog.FileName, destPath, true);
                _avatarPath = destPath;
                ProfileAvatar.Source = new BitmapImage(new Uri(destPath));
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var profile = new HttpChatService.UserProfile
            {
                Nickname = NicknameBox.Text,
                AvatarPath = _avatarPath,
                BirthDate = BirthDatePicker.SelectedDate
            };

            await service.UpdateUserProfile(Session.CurrentUser, profile);

            UserSettings.Instance.Nickname = NicknameBox.Text;
            UserSettings.Instance.BirthDate = BirthDatePicker.SelectedDate;
            if (!string.IsNullOrEmpty(_avatarPath))
                UserSettings.Instance.AvatarPath = _avatarPath;
            UserSettings.Instance.Save();

            _parent.UpdateUserInfo();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}