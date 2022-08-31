using System.Collections.Generic;

namespace ChatApp.Models
{
    public class ChatViewModel
    {
        public string GroupId { get; set; }
        public bool IsGroupActive { get; set; }
        public string RecipientId { get; set; }
        public string RecipientName { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
    }
}
