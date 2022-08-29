using System;
using System.Collections.Generic;

namespace ChatApp.Models
{
    public partial class User
    {
        public User()
        {
            GroupFromUsers = new HashSet<Group>();
            GroupToUsers = new HashSet<Group>();
            UserRoles = new HashSet<UserRole>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FullName { get; set; }
        public string Username { get; set; }
        public string StudentId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Password { get; set; }

        public virtual ICollection<Group> GroupFromUsers { get; set; }
        public virtual ICollection<Group> GroupToUsers { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<ContactRequest> ContactRequests { get; set; }
    }
}
