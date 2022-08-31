using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ChatApp.Models;
using ChatApp.Data;
using ChatApp.Supporters.Constants;
using ChatApp.Services;
using ChatApp.Models.DTO;

namespace ChatApp.Controllers
{
    [Authorize(Roles = RoleName.ADMIN)]
    public class AdminController : Controller
    {
        private readonly ChatAppImplementationContext dbContext;
        private readonly UserManager<User> userManager;
        private readonly IChatService chatService;

        public AdminController(ChatAppImplementationContext dbContext, UserManager<User> userManager, IChatService chatService)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.chatService = chatService;
        }

        public async Task<IActionResult> Index(string searchKey, string roleFilter)
        {
            var users = dbContext.Users.AsNoTracking().ToList();
            var viewModel = new List<AdminViewModel>();  
            foreach (var user in users)
            {
                var role = (await userManager.GetRolesAsync(user)).FirstOrDefault();
                if(RoleName.USER == role || RoleName.COLLABORATOR == role)
                {
                    viewModel.Add(await AdminViewModel.GetFromUser(user, role, chatService));
                }
                
            }
            if(roleFilter != null && roleFilter != "All")
            {
                viewModel = RoleFilter(viewModel, roleFilter);
            }
            if(searchKey != null)
            {
                viewModel = UserNameFilter(viewModel, searchKey);
            }    
            return View(viewModel);
        }
        private List<AdminViewModel> RoleFilter(List<AdminViewModel> source, string roleFilter)
        {
            var result = new List<AdminViewModel>();
            foreach(var model in source)
            {
                if(model.Role == roleFilter)
                {
                    result.Add(model);
                }
            }
            return result;
        }
        private List<AdminViewModel> UserNameFilter(List<AdminViewModel> source, string searchKey)
        {
            var result = new List<AdminViewModel>();
            foreach(var model in source)
            {
                if(model.Username.Contains(searchKey))
                {
                    result.Add(model);
                }
            }
            return result;
        }

        [HttpGet]
        public IActionResult CreateRespondent()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateRespondent(RegisterDTO registerDTO)
        {
            if(registerDTO.Password != registerDTO.ConfirmPassword)
            {
                TempData["confirmPasswordError"] = "Password not match";
                return View(registerDTO);
            }
            if(ModelState.IsValid)
            {
                var user = new User
                {
                    Username = registerDTO.Username,
                    Password = registerDTO.Password,
                    Email = registerDTO.Email,
                    FullName = registerDTO.FullName,
                    Phone = registerDTO.Phone,
                    StudentId = registerDTO.StudentId,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(user, registerDTO.Password);
                if(result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, RoleName.COLLABORATOR);
                    TempData["Success"] = "Add respondent successfully";
                }
            }
            else
            {
                TempData["generalError"] = "Add error, model not valid, please input again";
            }    
            return View(registerDTO);
        }

        public async Task<IActionResult> Interdict(string userId)
        {
            var user = dbContext.Users.Where(user => user.Id == userId).FirstOrDefault();
            try
            {
                user.IsActive = false;
                dbContext.Update(user);
                await dbContext.SaveChangesAsync();
                TempData["Success"] = "Interdict user " + user.Username + " successfully";
            }
            catch(Exception ex)
            {
                TempData["Error"] = "Interdict user" + user.Username + " error";
            }
            
            return RedirectToAction("Index", "Admin");
        }
        public async Task<IActionResult> Permit(string userId)
        {
            var user = dbContext.Users.Where(user => user.Id == userId).FirstOrDefault();
            try
            {
                user.IsActive = true;
                dbContext.Update(user);
                await dbContext.SaveChangesAsync();
                TempData["Success"] = "Permit user " + user.Username + " successfully";
            }
            catch(Exception ex)
            {
                TempData["Error"] = "Permit user" + user.Username + " error";
            }

            return RedirectToAction("Index", "Admin");
        }

    }
}
