using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageApp
{
    public class FileDatabase
    {
        private static FileDatabase? _instance;
        public static FileDatabase Instance => _instance ??= new FileDatabase();

        private readonly object _lock = new object();
        private string dataFolder;

        private FileDatabase()
        {
            dataFolder = Path.Combine(Environment.CurrentDirectory, "AppData");
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            LoadAllData();
        }

        private List<UserData> _users = new();
        private List<ChatData> _chats = new();
        private List<MessageData> _messages = new();
        private List<TodoItemData> _todos = new();

        private int _nextUserId = 1;
        private int _nextChatId = 1;
        private int _nextMessageId = 1;
        private int _nextTodoId = 1;

        public class UserData
        {
            public int Id { get; set; }
            public string Login { get; set; } = "";
            public string Password { get; set; } = "";
            public string Nickname { get; set; } = "";
            public string AvatarPath { get; set; } = "";
            public DateTime? BirthDate { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.Now;
        }

        public class ChatData
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public string CreatedBy { get; set; } = "";
        }

        public class MessageData
        {
            public int Id { get; set; }
            public int ChatId { get; set; }
            public string Username { get; set; } = "";
            public string Text { get; set; } = "";
            public DateTime Timestamp { get; set; } = DateTime.Now;
            public int? UserId { get; set; }
        }

        public class TodoItemData
        {
            public int Id { get; set; }
            public int ChatId { get; set; }
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public bool IsCompleted { get; set; }
            public string AssignedTo { get; set; } = "";
            public string CreatedBy { get; set; } = "";
            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public DateTime? DueDate { get; set; }
        }

        private void LoadAllData()
        {
            _users = LoadFromFile<List<UserData>>("users.json") ?? new List<UserData>();
            _chats = LoadFromFile<List<ChatData>>("chats.json") ?? new List<ChatData>();
            _messages = LoadFromFile<List<MessageData>>("messages.json") ?? new List<MessageData>();
            _todos = LoadFromFile<List<TodoItemData>>("todos.json") ?? new List<TodoItemData>();

            if (_users.Any()) _nextUserId = _users.Max(u => u.Id) + 1;
            if (_chats.Any()) _nextChatId = _chats.Max(c => c.Id) + 1;
            if (_messages.Any()) _nextMessageId = _messages.Max(m => m.Id) + 1;
            if (_todos.Any()) _nextTodoId = _todos.Max(t => t.Id) + 1;
        }

        private void SaveAllData()
        {
            SaveToFile("users.json", _users);
            SaveToFile("chats.json", _chats);
            SaveToFile("messages.json", _messages);
            SaveToFile("todos.json", _todos);
        }

        private T? LoadFromFile<T>(string filename)
        {
            string path = Path.Combine(dataFolder, filename);
            if (!File.Exists(path)) return default;

            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return default;
            }
        }

        private void SaveToFile<T>(string filename, T data)
        {
            string path = Path.Combine(dataFolder, filename);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }


        public async Task<bool> RegisterUser(string login, string password, string nickname = "")
        {
            await Task.Delay(1);

            lock (_lock)
            {
                if (_users.Any(u => u.Login == login))
                    return false;

                var user = new UserData
                {
                    Id = _nextUserId++,
                    Login = login,
                    Password = password,
                    Nickname = string.IsNullOrEmpty(nickname) ? login : nickname
                };

                _users.Add(user);
                SaveAllData();
                return true;
            }
        }

        public async Task<UserData?> GetUser(string login)
        {
            await Task.Delay(1);

            lock (_lock)
            {
                return _users.FirstOrDefault(u => u.Login == login);
            }
        }

        public async Task<bool> ValidateUser(string login, string password)
        {
            await Task.Delay(1);

            lock (_lock)
            {
                return _users.Any(u => u.Login == login && u.Password == password);
            }
        }

        public async Task<bool> UpdateUser(string login, string nickname, string avatarPath, DateTime? birthDate)
        {
            await Task.Delay(1);

            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Login == login);
                if (user == null) return false;

                if (!string.IsNullOrEmpty(nickname))
                    user.Nickname = nickname;
                if (!string.IsNullOrEmpty(avatarPath))
                    user.AvatarPath = avatarPath;
                if (birthDate.HasValue)
                    user.BirthDate = birthDate;

                SaveAllData();
                return true;
            }
        }


        public async Task<ChatData> CreateChat(string name, string createdBy)
        {
            await Task.Delay(1);

            lock (_lock)
            {
                var chat = new ChatData
                {
                    Id = _nextChatId++,
                    Name = name,
                    CreatedBy = createdBy
                };

                _chats.Add(chat);
                SaveAllData();
                return chat;
            }
        }

        public async Task<List<ChatData>> GetChats()
        {
            await Task.Delay(1);

            lock (_lock)
            {
                return _chats.OrderByDescending(c => c.CreatedAt).ToList();
            }
        }


        public async Task<MessageData> SaveMessage(int chatId, string username, string text)
        {
            await Task.Delay(1);

            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Login == username);

                var message = new MessageData
                {
                    Id = _nextMessageId++,
                    ChatId = chatId,
                    Username = username,
                    Text = text,
                    Timestamp = DateTime.Now,
                    UserId = user?.Id
                };

                _messages.Add(message);
                SaveAllData();
                return message;
            }
        }

        public async Task<List<MessageData>> GetMessages(int chatId, int limit = 100)
        {
            await Task.Delay(1);

            lock (_lock)
            {
                return _messages
                    .Where(m => m.ChatId == chatId)
                    .OrderBy(m => m.Timestamp)
                    .Take(limit)
                    .ToList();
            }
        }


        public async Task<TodoItemData> CreateTodo(int chatId, string title, string description,
                                                     string createdBy, DateTime? dueDate)
        {
            await Task.Delay(1);

            lock (_lock)
            {
                var todo = new TodoItemData
                {
                    Id = _nextTodoId++,
                    ChatId = chatId,
                    Title = title,
                    Description = description,
                    IsCompleted = false,
                    CreatedBy = createdBy,
                    AssignedTo = createdBy,
                    DueDate = dueDate
                };

                _todos.Add(todo);
                SaveAllData();
                return todo;
            }
        }

        public async Task<List<TodoItemData>> GetTodos(int chatId)
        {
            await Task.Delay(1);

            lock (_lock)
            {
                return _todos
                    .Where(t => t.ChatId == chatId)
                    .OrderBy(t => t.IsCompleted)
                    .ThenBy(t => t.DueDate)
                    .ToList();
            }
        }

        public async Task<bool> UpdateTodoStatus(int todoId, bool isCompleted)
        {
            await Task.Delay(1);

            lock (_lock)
            {
                var todo = _todos.FirstOrDefault(t => t.Id == todoId);
                if (todo == null) return false;

                todo.IsCompleted = isCompleted;
                SaveAllData();
                return true;
            }
        }

        public async Task<bool> DeleteTodo(int todoId)
        {
            await Task.Delay(1);

            lock (_lock)
            {
                var todo = _todos.FirstOrDefault(t => t.Id == todoId);
                if (todo == null) return false;

                _todos.Remove(todo);
                SaveAllData();
                return true;
            }
        }
    }
}