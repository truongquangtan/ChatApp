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

        public async Task<IActionResult> Index()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account");
                }
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

                var groups = await chatService.GetAllGroupsContain(user);

                var chatViewModels = new List<ChatViewModel>();

                foreach(var group in groups)
                {
                    var message = await chatService.GetLastMessageOfGroup(group.Id);
                    if(message != null)
                    {
                        ChatViewModel chat = new ChatViewModel
                        {
                            GroupId = group.Id,
                            IsGroupActive = group.IsActive,
                            LastMessage = message.Text,
                            LastMessageTime = message.CreatedAt,
                            RecipientId = role == RoleName.USER ? group.ToUserId : group.FromUserId,
                            RecipientName = role == RoleName.USER ? group.ToUserName : group.FromUserName
                        };
                        chatViewModels.Add(chat);
                    }    
                }

                var model = new FirstMessageModel()
                {
                    AuthorId = user.Id,
                    AuthorRole = role,
                    GroupsMessage = chatViewModels.OrderByDescending(chat => chat.LastMessageTime).ToList()
                };
                return View(model);
            }
            catch(Exception ex)
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> Chat(string groupId)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account");
                }
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

                var group = dbContext.Groups.Find(groupId);

                if(group == null)
                {
                    throw new Exception("Can not find group");
                }    

                if(group.FromUserId != user.Id && group.ToUserId != user.Id)
                {
                    throw new Exception("User is not in group");
                }
                var allMessages = await chatService.GetAllMessagesInGroupAsync(groupId);

                var model = new MainViewModel
                {
                    GroupId = groupId,
                    IsGroupActive = group.IsActive,
                    AuthorUserId = user.Id,
                    AuthorRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                    RecipientName = role == RoleName.USER ? group.ToUserName : group.FromUserName,
                    RecipientId = role == RoleName.USER ? group.ToUserId : group.FromUserId,
                    MyMessages = role == RoleName.USER ? allMessages.Where(message => message.IsFromRespondent == false).ToList() : allMessages.Where(message => message.IsFromRespondent).ToList(),
                    OtherMessages = role == RoleName.USER ? allMessages.Where(message => message.IsFromRespondent).ToList() : allMessages.Where(message => message.IsFromRespondent == false).ToList()
                };

                return View(model);
            } catch (Exception ex)
            {
                return NotFound();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}