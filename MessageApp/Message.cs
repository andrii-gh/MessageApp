using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageApp
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string Username { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime Timestamp { get; set; }

        public override string ToString() => $"[{Timestamp:HH:mm:ss}] {Username}: {Text}";
    }
}
