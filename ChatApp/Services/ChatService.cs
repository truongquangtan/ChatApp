using ChatApp.Models;
using ChatApp.Models.DTO;
using ChatApp.Data;
using ChatApp.Supporters.Constants;

using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services
{
    public class ChatService : IChatService
    {
        private readonly ChatAppImplementationContext dbContext;

        public ChatService(ChatAppImplementationContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Message> AddMessageAsync(Group group, string message, bool isFromRespondent)
        {
            Message messageEntity = new Message
            {
                GroupId = group.Id,
                Text = message,
                CreatedAt = DateTime.Now,
                IsFromRespondent = isFromRespondent
            };
            await dbContext.AddAsync(messageEntity);
            await dbContext.SaveChangesAsync();
            return messageEntity;
        }

        public async Task<Group> CreateGroupAsync(User user, User recipient)
        {
            var group = new Group()
            {
                FromUserId = user.Id,
                ToUserId = recipient.Id,
                FromUserName = user.Username,
                ToUserName = recipient.Username,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            await dbContext.Groups.AddAsync(group);
            await dbContext.SaveChangesAsync();
            return group;
        }

        public async Task<List<Message>> GetAllMessagesInGroupsAsync(List<string> groupIdList)
        {
            return await dbContext.Messages.Where(message => groupIdList.Contains(message.GroupId)).Include(message => message.Group).ToListAsync();
        }

        public async Task<List<Group>> GetAllGroupsContain(User user)
        {
            var groupList = await dbContext.Groups.Where(group => group.FromUserId == user.Id || group.ToUserId == user.Id).Include(group => group.FromUser).Include(group => group.ToUser).ToListAsync();
            return groupList;
        }

        public async Task<GroupDTO> GetContactInfomationAsync(User user)
        {
            var groupList = await dbContext.Groups.Where(group => group.FromUserId == user.Id || group.ToUserId == user.Id).Include(group => group.FromUser).Include(group => group.ToUser).ToListAsync();
            var users = GetAllUserInGroupsAsync(groupList);
            var groupDTO = new GroupDTO
            {
                ContactGroupIdList = groupList.Select(group => group.Id).ToList(),
                ContactUserList = users
            };
            return groupDTO;
        }
        private List<User> GetAllUserInGroupsAsync(List<Group> groups)
        {
            List<User> sendUsers = groups.Select(group => group.FromUser).ToList();
            List<User> recipientUsers = groups.Select(group => group.ToUser).ToList();

            List<User> users = new();
            users.AddRange(sendUsers);

            foreach(var user in recipientUsers)
            {
                if(!users.Contains(user))
                {
                    users.Add(user);
                }
            }
            return users;
        }
        
        public async Task<List<Message>> GetAllMessagesInGroupAsync(string groupId)
        {
            var messages = await dbContext.Messages.AsNoTracking().Where(message => message.GroupId == groupId).ToListAsync();
            return messages;
        }
        public async Task<Message> GetLastMessageOfGroup(string groupId)
        {
            var message = await dbContext.Messages.Where(message => message.GroupId == groupId).OrderByDescending(message => message.CreatedAt).FirstOrDefaultAsync();
            return message;
        }

        public async Task<bool> CheckGroupBeingEndRequested(ChatAppImplementationContext dbContext, Group group)
        {
            var endRequest = dbContext.EndConversationRequests.Where(request => request.GroupId == group.Id).FirstOrDefault();
            if (endRequest != null && endRequest.CreatedAt.AddMinutes(TimeRequestExist.TIME_EXIST_IN_MINUTE).CompareTo(DateTime.Now) < 0)
            {
                using var transaction = dbContext.Database.BeginTransaction();
                try
                {
                    dbContext.EndConversationRequests.Remove(endRequest);
                    group.IsActive = false;
                    dbContext.Groups.Update(group);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
            return false;
        }

    }
}
