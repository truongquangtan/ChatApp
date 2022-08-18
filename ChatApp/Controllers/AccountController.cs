using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

using ChatApp.Models;
using ChatApp.Data;
using ChatApp.Models.DTO;
using ChatApp.Supporters.Constants;

using Microsoft.AspNetCore.Authorization;

namespace ChatApp.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if(User != null && User.Identity.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            User? user = null;
            if (username != null)
            {
                user = await _userManager.FindByNameAsync(username);
            }

            if(user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);

                if(result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            TempData["err"] = "Incorrect username or password";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO request)
        {
            if(request.Password != request.ConfirmPassword)
            {
                TempData["confirmPasswordError"] = "Password not match";
                return View(request);
            }    
            if(ModelState.IsValid)
            {
                var user = new User
                {
                    Username = request.Username,
                    Password = request.Password,
                    StudentId = request.StudentId,
                    FullName = request.FullName,
                    Email = request.Email,
                    Phone = request.Phone
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, RoleName.USER);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var errors = result.Errors.ToList();
                    var errorString = "";
                    foreach(var error in errors)
                    {
                        errorString += error.Description;
                    }
                    TempData["generalError"] = errorString;
                }    
            }
            else
            {
                TempData["generalError"] = "Your input is not valid, please fill all require field";
            }    
            return RedirectToAction("Register", "Account");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
