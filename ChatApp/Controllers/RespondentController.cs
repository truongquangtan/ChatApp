using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

using Microsoft.EntityFrameworkCore;

using ChatApp.Models;
using ChatApp.Data;
using ChatApp.Hubs;
using ChatApp.Models.DTO;

namespace ChatApp.Controllers
{
    public class RespondentController : Controller
    {
        private readonly ChatAppImplementationContext dbContext;
        private readonly IHubContext<ChatHub> _hubContext;

        public RespondentController(ChatAppImplementationContext dbContext, IHubContext<ChatHub> hubContext)
        {
            this.dbContext = dbContext;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestContact(string userId)
        {
            var userRequested = await dbContext.Users.Where(user => user.Id == userId).FirstOrDefaultAsync();

            var userDTO = new UserDTO
            {
                Email = userRequested.Email,
                FullName = userRequested.FullName,
                Phone = userRequested.Phone,
                UserId = userRequested.Id,
                UserName = userRequested.Username
            };

            await _hubContext.Clients.Group("Respondent").SendAsync("ReceiveRequest", userDTO.UserId, userDTO.FullName, userDTO.Email, userDTO.Phone);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> GetContact(string userId)
        {
            
            return View();
        }
    }
}
