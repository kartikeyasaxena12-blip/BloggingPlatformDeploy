using System.ComponentModel.DataAnnotations;

namespace NewsletterService.Dtos
{
    public class SubscribeRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
