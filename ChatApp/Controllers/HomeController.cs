using ChatApp.Data;
using ChatApp.Models;
using ChatApp.Services;
using ChatApp.Supporters.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
                    if(group.IsBeingEndRequested && group.IsActive)
                    {
                        await chatService.CheckGroupBeingEndRequested(dbContext, group);
                    }    
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
                var groupsMessage = new List<ChatViewModel>();
                groupsMessage.AddRange(chatViewModels.Where(chat => chat.IsGroupActive).OrderByDescending(chat => chat.LastMessageTime).ToList());
                groupsMessage.AddRange(chatViewModels.Where(chat => chat.IsGroupActive == false).OrderByDescending(chat => chat.LastMessageTime).ToList());

                var model = new FirstMessageModel()
                {
                    AuthorId = user.Id,
                    AuthorRole = role,
                    GroupsMessage = groupsMessage
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

                var endRequest = dbContext.EndConversationRequests.Where(request => request.GroupId == groupId).FirstOrDefault();
                if (endRequest != null && endRequest.CreatedAt.AddMinutes(TimeRequestExist.TIME_EXIST_IN_MINUTE).CompareTo(DateTime.Now) < 0)
                {
                    dbContext.EndConversationRequests.Remove(endRequest);
                    group.IsActive = false;
                    dbContext.Groups.Update(group);
                    await dbContext.SaveChangesAsync();
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