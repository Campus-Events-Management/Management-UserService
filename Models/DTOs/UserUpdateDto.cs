using System.ComponentModel.DataAnnotations;

namespace EventManagement.UserService.Models.DTOs
{
    public class UserUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        // Optional password change
        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }
        
        [Compare("Password")]
        public string? ConfirmPassword { get; set; }
    }
} 