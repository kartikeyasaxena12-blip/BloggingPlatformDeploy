using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Role { get; set; } = "Reader"; // Admin, Author, Reader

        [MaxLength(200)]
        public string? FullName { get; set; }

        public string? Bio { get; set; }

        public string? AvatarUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsEmailVerified { get; set; } = false;

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
