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
                var recipient = await dbContext.Users.SingleOrDefaultAsync(user => user.Id == to);
                
                if(recipient == null)
                {
                    throw new Exception("Recipient not existed");
                }

                Group group = await chatService.EnsureGroupAsync(user, recipient);

                Message message = await chatService.AddMessageAsync(group, text);

                await _hubContext.Clients.User(recipient.Id).SendAsync("ReceiveMessage", message.Text, message.CreatedAt.ToShortTimeString());
            } catch (Exception)
            {
                return StatusCode(500);
            }

            return Ok();
        }
    }
}
