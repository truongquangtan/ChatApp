using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Models
{
    public partial class Message
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Text { get; set; }

        public virtual User FromUserNavigation { get; set; }
        public virtual User ToUserNavigation { get; set; }
    }
}
