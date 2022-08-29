using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using ChatApp.Models;
using ChatApp.Data;
using ChatApp.Hubs;
using ChatApp.Models.DTO;
using ChatApp.Services;
using ChatApp.Supporters.Constants;

namespace ChatApp.Controllers
{
    public class RespondentController : Controller
    {
        private readonly ChatAppImplementationContext dbContext;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly UserManager<User> _userManager;
        private readonly IChatService chatService;

        public RespondentController(ChatAppImplementationContext dbContext, IHubContext<ChatHub> hubContext, UserManager<User> userManager, IChatService chatService)
        {
            this.dbContext = dbContext;
            _hubContext = hubContext;
            _userManager = userManager;
            this.chatService = chatService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RequestContact(string userId)
        {
            using var dbContext = new ChatAppImplementationContext();
            var lastContactRequest = dbContext.ContactRequests.AsNoTracking().Where(contact => contact.UserId == userId);
            if(lastContactRequest != null && lastContactRequest.Any())
            {
                return Ok();
            }

            var userRequested = await dbContext.Users.Where(user => user.Id == userId).FirstOrDefaultAsync();

            var userDTO = new UserDTO
            {
                Email = userRequested.Email,
                FullName = userRequested.FullName,
                Phone = userRequested.Phone,
                UserId = userRequested.Id,
                UserName = userRequested.Username
            };

            ContactRequest contactRequest = new ContactRequest { UserId = userId };

            dbContext.ContactRequests.Add(contactRequest);
            await dbContext.SaveChangesAsync();

            await _hubContext.Clients.Group(ChatApp.Supporters.Constants.GroupName.RESPONDENT_GROUP).SendAsync("ReceiveRequest", userDTO.UserId, userDTO.FullName, userDTO.Email, userDTO.Phone, contactRequest.Id);

            return Ok();
        }

        public async Task<IActionResult> GetContact()
        {
            using var dbContext = new ChatAppImplementationContext();
            var allContactRequest = await dbContext.ContactRequests.Where(request => true).Include(contact => contact.User).ToListAsync();
            var model = new List<GetContactModel>();
            foreach(var contact in allContactRequest)
            {
                model.Add(new GetContactModel()
                {
                    ContactId = contact.Id,
                    Email = contact.User.Email,
                    Phone = contact.User.Phone,
                    UserId = contact.UserId,
                    UserName = contact.User.FullName
                });
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SetContact(string userId, string connectionId, string contactId)
        {
            try
            {
                var contactRequest = await dbContext.ContactRequests.FindAsync(contactId);

                if(contactRequest == null)
                {
                    throw new Exception("The request is solved by another or removed.");
                }

                var userToContact = await _userManager.FindByIdAsync(userId);
                var authorUser = await _userManager.FindByNameAsync(User.Identity.Name);
                var group = await chatService.CreateGroupAsync(userToContact, recipient: authorUser);
                var firstMessage = await DoSetContactTransactionAsync(userToContact, authorUser, contactRequest, group, connectionId);

                return RedirectToAction("Chat", "Home", new { groupId = group.Id });
            } 
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("GetContact");
            }
        }

        private async Task<Message> DoSetContactTransactionAsync(User userToContact, User authorUser, ContactRequest contactRequest, Group group, string connectionId)
        {
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    dbContext.Users.Update(userToContact);
                    dbContext.ContactRequests.Remove(contactRequest);
                    var message = await chatService.AddMessageAsync(group, InitialMessage.TEXT, isFromRespondent: true);
                    await dbContext.SaveChangesAsync();

                    await _hubContext.Clients.User(userToContact.Id).SendAsync("ReceiveMessage", message.Text, message.CreatedAt.ToShortTimeString());
                    await _hubContext.Clients.Group(GroupName.RESPONDENT_GROUP).SendAsync("Remove", userToContact.Id);
                    await _hubContext.Groups.RemoveFromGroupAsync(connectionId, GroupName.RESPONDENT_GROUP);
                    await _hubContext.Clients.User(userToContact.Id).SendAsync("ReloadPage", group.Id);

                    await transaction.CommitAsync();
                    return message;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return null;
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRequest()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await RemoveLastContactRequestOfUser(user.Id);
            await _hubContext.Clients.Group(GroupName.RESPONDENT_GROUP).SendAsync("Remove", user.Id);
            return Ok();
        }
        private async Task RemoveLastContactRequestOfUser(string userId)
        {
            using var dbContext = new ChatAppImplementationContext();
            var lastContactRequest = dbContext.ContactRequests.AsNoTracking().Where(contact => contact.UserId == userId);
            try
            {
                if (lastContactRequest != null)
                {
                    dbContext.ContactRequests.RemoveRange(lastContactRequest);
                    await dbContext.SaveChangesAsync();
                }
            } catch (DbUpdateConcurrencyException)
            {

            }
        }
    }
}
