using ChatApp.Models;
using ChatApp.Services;

namespace ChatApp.Models
{
    public class AdminViewModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string MessageSent { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; }

        public static async Task<AdminViewModel> GetFromUser(User user, string role, IChatService chatService)
        {
            return new AdminViewModel
            {
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive == null ? false : user.IsActive.Value,
                MessageSent = (await chatService.CountMessageSent(user)).ToString(),
                UserId = user.Id,
                Username = user.Username,
                Role = role
            };
        }
    }
}
