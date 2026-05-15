using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MessageApp
{
    public partial class TodoWindow : Window
    {
        private HttpChatService service = new();
        private int currentChatId;
        private string currentUser;
        private List<TodoItem> todos = new();
        private System.Windows.Threading.DispatcherTimer refreshTimer;

        public TodoWindow(int chatId)
        {
            InitializeComponent();
            currentChatId = chatId;
            currentUser = Session.CurrentUser ?? "Unknown";
            DueDatePicker.SelectedDate = DateTime.Now.AddDays(7);

            LoadTodos();

            refreshTimer = new System.Windows.Threading.DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(5);
            refreshTimer.Tick += async (s, e) => await LoadTodos();
            refreshTimer.Start();
        }

        private async Task LoadTodos()
        {
            try
            {
                todos = await service.GetTodos(currentChatId);
                Dispatcher.Invoke(() =>
                {
                    TodoList.ItemsSource = todos.OrderBy(x => x.IsCompleted).ThenBy(x => x.DueDate).ToList();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tasks: {ex.Message}");
            }
        }

        private async void AddTodo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Please enter a title", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var todo = new TodoItem
            {
                Title = TitleBox.Text,
                Description = DescriptionBox.Text,
                DueDate = DueDatePicker.SelectedDate,
                IsCompleted = false,
                CreatedBy = currentUser,
                AssignedTo = currentUser,
                ChatId = currentChatId
            };

            await service.CreateTodo(todo);

            TitleBox.Clear();
            DescriptionBox.Clear();
            DueDatePicker.SelectedDate = DateTime.Now.AddDays(7);

            await LoadTodos();
        }

        private async void TodoCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is int id)
            {
                bool isCompleted = checkBox.IsChecked ?? false;
                await service.UpdateTodoStatus(id, isCompleted);
                await LoadTodos();
            }
        }

        private async void DeleteTodo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int id)
            {
                var result = MessageBox.Show("Are you sure you want to delete this task?", "Confirm",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await service.DeleteTodo(id);
                    await LoadTodos();
                }
            }
        }

        private async void EditTodo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int id)
            {
                var todo = todos.FirstOrDefault(x => x.Id == id);
                if (todo != null)
                {
                    var dialog = new EditTodoDialog(todo);
                    dialog.Owner = this;
                    if (dialog.ShowDialog() == true)
                    {
                        await service.UpdateTodo(id, dialog.UpdatedTodo.Title, dialog.UpdatedTodo.Description, dialog.UpdatedTodo.DueDate);
                        await LoadTodos();
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            refreshTimer?.Stop();
            base.OnClosed(e);
        }
    }
}