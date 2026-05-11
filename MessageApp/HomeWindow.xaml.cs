using System.Timers;
using System.Windows;
using System.Windows.Threading;
﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MessageApp
{
    public partial class HomeWindow : Window
    {
        LocalServer server = new();
        HttpChatService service = new();

        bool menuOpen = true;
        int currentChatId = 0;
        DispatcherTimer timer = new();
        private LocalServer _server = new();
        private HttpChatService _service = new();
        private Timer _pollingTimer;
        private bool _menuOpen = true;
        private bool _isPolling = false;
        private List<string> _currentMessages = new List<string>();

        public HomeWindow()
        {
            InitializeComponent();

            _server.Start();

            LoginText.Text = $"Login: {Session.CurrentUser}";
            LoadChats();

            timer.Interval =
                TimeSpan.FromSeconds(2);

            timer.Tick += async (s, e) =>
            {
                if (currentChatId != 0)
                    await LoadMessages();
            };

            timer.Start();
        }

        private async void LoadChats()
        {
            ChatsList.ItemsSource =
                await service.GetChats();
        }

        private async void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            string name =
                "Chat " + DateTime.Now.Second;

            await service.CreateChat(name);

            LoadChats();
        }

        private async void ChatsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ChatsList.SelectedItem is Chat chat)
            {
                currentChatId = chat.Id;

                await LoadMessages();
            }
        }

        private async Task LoadMessages()
        {
            var msgs =
                await service.GetMessages(currentChatId);

            ChatBox.Clear();

            foreach (var msg in msgs)
            {
                ChatBox.AppendText(msg.Text + "\n");

            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _ = LoadMessagesAsync();
            StartPolling();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            StopPolling();
        }

        private void StartPolling()
        {
            _pollingTimer = new Timer(PollMessages, null, 2000, 3000);
        }

        private void StopPolling()
        {
            _pollingTimer?.Dispose();
            _pollingTimer = null;
        }

        private async void PollMessages(object state)
        {
            if (_isPolling) return;
            _isPolling = true;

            try
            {
                var messages = await _service.GetMessages();

                Dispatcher.Invoke(() =>
                {
                    foreach (var msg in messages)
                    {
                        if (!_currentMessages.Contains(msg))
                        {
                            _currentMessages.Add(msg);
                            ChatBox.AppendText(msg + "\n");
                        }
                    }
                    ChatBox.ScrollToEnd();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Polling error: {ex.Message}");
            }
            finally
            {
                _isPolling = false;
            }
        }

        private async Task LoadMessagesAsync()
        {
            try
            {
                var messages = await _service.GetMessages();

                Dispatcher.Invoke(() =>
                {
                    ChatBox.Clear();
                    _currentMessages.Clear();

                    foreach (var msg in messages)
                    {
                        _currentMessages.Add(msg);
                        ChatBox.AppendText(msg + "\n");
                    }
                    ChatBox.ScrollToEnd();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ChatBox.AppendText($"Помилка завантаження: {ex.Message}\n");
                });
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MsgBox.Text))
            {
                MessageBox.Show("Enter message");
                return;
            }

            string msg = $"{Session.CurrentUser}: {MsgBox.Text}";

            var sendButton = sender as System.Windows.Controls.Button;
            if (sendButton != null) sendButton.IsEnabled = false;

            try
            {
                await _service.Send(msg);

                Dispatcher.Invoke(() =>
                {
                    if (!_currentMessages.Contains(msg))
                    {
                        _currentMessages.Add(msg);
                        ChatBox.AppendText(msg + "\n");
                    }
                    ChatBox.ScrollToEnd();
                    MsgBox.Clear();
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ChatBox.AppendText($"Помилка надсилання: {ex.Message}\n");
                });
            }
            finally
            {
                if (sendButton != null)
                    Dispatcher.Invoke(() => sendButton.IsEnabled = true);
                Dispatcher.Invoke(() => MsgBox.Focus());
            }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            _menuOpen = !_menuOpen;

            if (_menuOpen)
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
            StopPolling();
            Session.CurrentUser = null;
            new MainWindow().Show();
            Close();
        }
    }
}
