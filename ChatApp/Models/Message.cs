using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Models
{
    public partial class Message
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string GroupId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Text { get; set; }

        public virtual Group Group { get; set; }
    }
}
