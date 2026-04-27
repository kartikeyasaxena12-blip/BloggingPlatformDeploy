using System.ComponentModel.DataAnnotations;

namespace AuthService.Dtos
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? FullName { get; set; }
    }
}
