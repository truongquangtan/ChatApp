using System.Collections.Generic;

namespace ChatApp.Models
{
    public class ChatViewModel
    {
        public string RecipientName { get; set; }
        public string RecipientId { get; set; }
        public List<Message> MyMessages { get; set; }
        public List<Message> OtherMessages { get; set; }
        public Message LastMessage { get; set; }
    }
}
