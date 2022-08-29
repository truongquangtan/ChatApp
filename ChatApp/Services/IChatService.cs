using ChatApp.Models;

namespace ChatApp.Services
{
    public interface IChatService
    {
        public Task<Group> CreateGroupAsync(User user, User recipient);
        public Task<Message> AddMessageAsync(Group group, string message, bool isFromRespondent);
        public Task<Models.DTO.GroupDTO> GetContactInfomationAsync(User user);
        public Task<List<Message>> GetAllMessagesInGroupsAsync(List<string> groupIdList);
        public Task<List<Message>> GetAllMessagesInGroupAsync(string groupId);
        public Task<List<Group>> GetAllGroupsContain(User user);
        public Task<Message> GetLastMessageOfGroup(string groupId);
    }
}
