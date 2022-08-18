using ChatApp.Models;

namespace ChatApp.Services
{
    public interface IChatService
    {
        public Task<Group> EnsureGroupAsync(User user, User recipient);
        public Task<Message> AddMessageAsync(Group group, string message);
        public Task<Models.DTO.GroupDTO> GetContactInfomationAsync(User user);
        public Task<List<Message>> GetAllMessagesInGroupsAsync(List<string> groupIdList);
    }
}
