using System.ComponentModel.DataAnnotations;

namespace NewsletterService.Models
{
    public class Subscriber
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>Token emailed to subscriber to confirm their address.</summary>
        public string ConfirmationToken { get; set; } = string.Empty;

        /// <summary>True once the subscriber clicks the confirmation link.</summary>
        public bool IsConfirmed { get; set; } = false;

        /// <summary>Token embedded in every notification email for one-click unsubscribe.</summary>
        public string UnsubscribeToken { get; set; } = string.Empty;

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}
