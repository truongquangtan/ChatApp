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

                var group = await dbContext.Groups.Where(group => group.Id == to).Include(group => group.FromUser).Include(group => group.ToUser).FirstOrDefaultAsync();

                if(group == null || group.IsActive == false)
                {
                    throw new Exception("Group is not existed");
                }

                var user = await _userManager.GetUserAsync(HttpContext.User);
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                if(user.Id != group.FromUserId && user.Id != group.ToUserId)
                {
                    throw new Exception("Group is not of user");
                }
                var recipient = role == RoleName.USER ? group.ToUser : group.FromUser;
                Message message = await chatService.AddMessageAsync(group, text, role != RoleName.USER);

                await _hubContext.Clients.User(recipient.Id).SendAsync("ReceiveMessage", message.Text, message.CreatedAt.ToShortTimeString());
            } catch (Exception)
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
                var request = new EndConversationRequest()
                {
                    GroupId = groupId,
                    IsConfimed = false,
                    RequestUserId = user.Id,
                    ConfirmUserId = group.FromUserId == user.Id ? group.ToUserId : group.FromUserId,
                    CreatedAt = DateTime.Now,
                    Id = Guid.NewGuid().ToString()
                };

                dbContext.EndConversationRequests.Add(request);
                await dbContext.SaveChangesAsync();

                await _hubContext.Clients.User(request.ConfirmUserId).SendAsync("RequestReached", request.Id);

                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500);
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
                        dbContext.EndConversationRequests.Remove(request);
                        dbContext.SaveChanges();
                        transaction.Commit();
                    }
                    catch(Exception)
                    {
                        transaction.Rollback();
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
