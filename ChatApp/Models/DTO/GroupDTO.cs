using System.Collections.Generic;

namespace ChatApp.Models.DTO
{
    public class GroupDTO
    {
        public List<string> ContactGroupIdList { get; set; }
        public List<User>  ContactUserList { get; set; }
    }
}
