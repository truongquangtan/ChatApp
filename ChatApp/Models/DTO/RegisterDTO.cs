using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models.DTO
{
    public class RegisterDTO
    {
        [Required]
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public string StudentId { get; set; }
        
    }
}
