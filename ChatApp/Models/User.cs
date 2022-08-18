using System;
using System.Collections.Generic;

namespace ChatApp.Models
{
    public partial class User
    {
        public User()
        {
            MessageFromUserNavigations = new HashSet<Message>();
            MessageToUserNavigations = new HashSet<Message>();
            UserRoles = new HashSet<UserRole>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string StudentId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public virtual ICollection<Message> MessageFromUserNavigations { get; set; }
        public virtual ICollection<Message> MessageToUserNavigations { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
