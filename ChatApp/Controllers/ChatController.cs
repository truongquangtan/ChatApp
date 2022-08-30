using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ChatApp.Models;
using ChatApp.Data;
using ChatApp.Hubs;
using ChatApp.Services;
using ChatApp.Supporters.Constants;

namespace ChatApp.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ChatAppImplementationContext dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IChatService chatService;

        public ChatController(ChatAppImplementationContext dbContext, UserManager<User> userManager, IHubContext<ChatHub> hubContext, IChatService chatService)
        {
            this.dbContext = dbContext;
            _userManager = userManager;
            _hubContext = hubContext;
            this.chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string to, string text)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                var group = await dbContext.Groups.Where(group => group.Id == to).Include(group => group.FromUser).Include(group => group.ToUser).FirstOrDefaultAsync();

                if(group == null || group.IsActive == false)
                {
                    await _hubContext.Clients.User(user.Id).SendAsync("ReloadPageToIndex");
                    throw new Exception("Group is not existed");
                }
                if (user.Id != group.FromUserId && user.Id != group.ToUserId)
                {
                    throw new Exception("Group is not of user");
                }

                if (group.IsBeingEndRequested)
                {
                    if(await chatService.CheckIfGroupBeingEndRequestedWasExpired(dbContext, group))
                    {
                        await _hubContext.Clients.User(user.Id).SendAsync("ReloadPageToIndex");
                        throw new Exception("The group is closed");
                    }
                }

                 var recipient = role == RoleName.USER ? group.ToUser : group.FromUser;
                 Message message = await chatService.AddMessageAsync(group, text, role != RoleName.USER);
                 await _hubContext.Clients.User(recipient.Id).SendAsync("ReceiveMessage", message.Text, message.CreatedAt.ToShortTimeString());
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> EndConversationRequest(string groupId)
        {
            try
            {
                using var dbContext = new ChatAppImplementationContext();

                var group = dbContext.Groups.Where(group => group.Id == groupId).FirstOrDefault();
                if (group == null)
                {
                    throw new Exception("Can not find group");
                }

                var preRequest = dbContext.EndConversationRequests.Where(request => request.GroupId == group.Id).FirstOrDefault();
                if (preRequest != null)
                {
                    throw new Exception("The request is created");
                }

                var user = await _userManager.GetUserAsync(HttpContext.User);
                if(await EndConversationRequestTransaction(user, group))
                {
                    return Ok();
                }
                else
                {
                    throw new Exception("Error in add request transaction");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500);
            }
        }

        private async Task<bool> EndConversationRequestTransaction(User user, Group group)
        {
            using var dbContext = new ChatAppImplementationContext();
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var request = new EndConversationRequest()
                {
                    GroupId = group.Id,
                    IsConfimed = false,
                    RequestUserId = user.Id,
                    ConfirmUserId = group.FromUserId == user.Id ? group.ToUserId : group.FromUserId,
                    CreatedAt = DateTime.Now,
                    Id = Guid.NewGuid().ToString()
                };
                dbContext.EndConversationRequests.Add(request);

                group.IsBeingEndRequested = true;
                dbContext.Groups.Update(group);

                await dbContext.SaveChangesAsync();

                await _hubContext.Clients.User(request.ConfirmUserId).SendAsync("RequestReached", request.Id);
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                return false;
            }
        }


        public async Task<IActionResult> ConfirmEndConversation(string requestId)
        {
            try
            {
                using var dbContext = new ChatAppImplementationContext();
                var request = dbContext.EndConversationRequests.FirstOrDefault(request => request.Id == requestId);
                if(request == null)
                {
                    return NotFound();
                }
                var group = dbContext.Groups.Find(request.GroupId);
                
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        group.IsActive = false;
                        dbContext.Groups.Update(group);

                        dbContext.EndConversationRequests.Remove(request);

                        dbContext.SaveChanges();
                        transaction.Commit();
                    }
                    catch(Exception)
                    {
                        transaction.Rollback();
                        throw new Exception("Error when execute transaction");
                    }
                }

                await _hubContext.Clients.User(request.RequestUserId).SendAsync("ReloadPageToIndex");

            }
            catch (Exception ex)
            {
                return NotFound();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
