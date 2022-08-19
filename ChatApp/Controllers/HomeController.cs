using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

using ChatApp.Models;
using ChatApp.Supporters.Constants;
using ChatApp.Data;
using ChatApp.Hubs;
using ChatApp.Services;
using ChatApp.Models.DTO;

namespace ChatApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ChatAppImplementationContext dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IChatService chatService;

        public HomeController(ChatAppImplementationContext dbContext, UserManager<User> userManager, IChatService chatService)
        {
            this.dbContext = dbContext;
            _userManager = userManager;
            this.chatService = chatService;
        }

        public async Task<IActionResult> Index(string? toShowId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var user = await _userManager.GetUserAsync(HttpContext.User);

            GroupDTO groupDTO = await chatService.GetContactInfomationAsync(user);

            var allMessages = await chatService.GetAllMessagesInGroupsAsync(groupDTO.ContactGroupIdList);

            var chats = new List<ChatViewModel>();
            foreach (var i in groupDTO.ContactUserList)
            {
                if (i == user) continue;

                var chat = new ChatViewModel()
                {
                    MyMessages = allMessages.Where(x => x.Group.FromUserId == user.Id && x.Group.ToUserId == i.Id).ToList(),
                    OtherMessages = allMessages.Where(x => x.Group.FromUserId == i.Id && x.Group.ToUserId == user.Id).ToList(),
                    RecipientName = i.FullName,
                    RecipientId = i.Id
                };

                var chatMessages = new List<Message>();
                chatMessages.AddRange(chat.MyMessages);
                chatMessages.AddRange(chat.OtherMessages);

                chat.LastMessage = chatMessages.OrderByDescending(c => c.CreatedAt).FirstOrDefault();

                chats.Add(chat);
            }

            var model = new MainViewModel
            {
                AuthorUserId = user.Id,
                ChatViewModels = chats,
                ShowChatMessageForUserId = toShowId
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}