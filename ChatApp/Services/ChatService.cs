﻿using ChatApp.Models;
using ChatApp.Models.DTO;
using ChatApp.Data;

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

        public async Task<Message> AddMessageAsync(Group group, string message)
        {
            Message messageEntity = new Message
            {
                GroupId = group.Id,
                Text = message,
                CreatedAt = DateTime.Now
            };
            await dbContext.AddAsync(messageEntity);
            await dbContext.SaveChangesAsync();
            return messageEntity;
        }

        public async Task<Group> EnsureGroupAsync(User user, User recipient)
        {
            Group? group = await dbContext.Groups.Where(group => group.FromUserId == user.Id && group.ToUserId == recipient.Id).FirstOrDefaultAsync();
            if (group == null)
            {
                group = new Group()
                {
                    FromUserId = user.Id,
                    ToUserId = recipient.Id,
                    FromUserName = user.Username,
                    ToUserName = recipient.Username
                };
                await dbContext.Groups.AddAsync(group);
                await dbContext.SaveChangesAsync();
            }
            return group;
        }

        public async Task<List<Message>> GetAllMessagesInGroupsAsync(List<string> groupIdList)
        {
            return await dbContext.Messages.Where(message => groupIdList.Contains(message.GroupId)).Include(message => message.Group).ToListAsync();
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
    }
}