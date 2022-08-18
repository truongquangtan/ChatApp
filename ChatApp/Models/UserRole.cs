using System;
using System.Collections.Generic;

namespace ChatApp.Models
{
    public partial class UserRole
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }

        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}
