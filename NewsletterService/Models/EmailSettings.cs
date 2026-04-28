namespace NewsletterService.Models
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "InkWell Newsletter";
        /// <summary>
        /// When true, emails are printed to console instead of being sent via SMTP.
        /// Set to false with real credentials to send actual emails.
        /// </summary>
        public bool SandboxMode { get; set; } = true;
        /// <summary>
        /// Base URL of the NewsletterService — used to build confirmation/unsubscribe links.
        /// </summary>
        public string ServiceBaseUrl { get; set; } = "http://localhost:5600";
        public string FrontendUrl { get; set; } = "http://localhost:3000";
    }
}
