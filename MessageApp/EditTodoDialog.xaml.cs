using System.Windows;

namespace MessageApp
{
    public partial class EditTodoDialog : Window
    {
        public TodoItem UpdatedTodo { get; private set; }

        public EditTodoDialog(TodoItem todo)
        {
            InitializeComponent();
            UpdatedTodo = new TodoItem
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                DueDate = todo.DueDate,
                IsCompleted = todo.IsCompleted
            };

            TitleBox.Text = todo.Title;
            DescriptionBox.Text = todo.Description;
            DueDatePicker.SelectedDate = todo.DueDate;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Please enter a title", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UpdatedTodo.Title = TitleBox.Text;
            UpdatedTodo.Description = DescriptionBox.Text;
            UpdatedTodo.DueDate = DueDatePicker.SelectedDate;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}