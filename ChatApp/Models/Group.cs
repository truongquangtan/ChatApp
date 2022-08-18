using System;
using System.Collections.Generic;

namespace ChatApp.Models
{
    public partial class Group
    {
        public Group()
        {
            Messages = new HashSet<Message>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string? FromUserName { get; set; }
        public string? ToUserName { get; set; }

        public virtual User FromUser { get; set; }
        public virtual User ToUser { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
