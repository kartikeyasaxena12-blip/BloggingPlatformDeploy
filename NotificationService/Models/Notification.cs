using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public string Type { get; set; } = "INFO"; // e.g. INFO, SUCCESS, WARNING

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
