using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NewsletterService.Models;

namespace NewsletterService.Services
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string toEmail, string confirmationLink);
        Task SendNewPostNotificationAsync(string toEmail, string unsubscribeLink, string postTitle, string authorName, string postUrl);
    }

    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        // ── Confirmation Email ─────────────────────────────────────────────────
        public async Task SendConfirmationEmailAsync(string toEmail, string confirmationLink)
        {
            var subject = "✅ Confirm your InkWell subscription";
            var htmlBody = BuildConfirmationHtml(confirmationLink);
            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        // ── New-Post Notification Email ─────────────────────────────────────────
        public async Task SendNewPostNotificationAsync(
            string toEmail, string unsubscribeLink,
            string postTitle, string authorName, string postUrl)
        {
            var subject = $"📖 New on InkWell: {postTitle}";
            var htmlBody = BuildNotificationHtml(postTitle, authorName, postUrl, unsubscribeLink);
            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        // ── Core Send ──────────────────────────────────────────────────────────
        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (_settings.SandboxMode)
            {
                _logger.LogInformation(
                    "[EMAIL SANDBOX] To: {To} | Subject: {Subject}", toEmail, subject);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(new MailboxAddress(string.Empty, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("[EMAIL SENT] To: {To} | Subject: {Subject}", toEmail, subject);
        }

        // ── HTML Templates ─────────────────────────────────────────────────────
        private static string BuildConfirmationHtml(string confirmationLink) => $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>Confirm your subscription</title>
            </head>
            <body style="margin:0;padding:0;background:#080b14;font-family:'Segoe UI',sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#080b14;padding:40px 0;">
                <tr><td align="center">
                  <table width="560" cellpadding="0" cellspacing="0" style="background:#131825;border-radius:16px;overflow:hidden;border:1px solid rgba(255,255,255,0.06);">
                    <!-- Header gradient bar -->
                    <tr><td style="height:4px;background:linear-gradient(90deg,#6c63ff,#a78bfa,#e879f9);"></td></tr>
                    <!-- Logo -->
                    <tr><td align="center" style="padding:36px 40px 0;">
                      <p style="font-size:2.2rem;margin:0;">✍️</p>
                      <h1 style="margin:10px 0 0;font-size:1.6rem;font-weight:800;background:linear-gradient(135deg,#6c63ff,#a78bfa);-webkit-background-clip:text;-webkit-text-fill-color:transparent;background-clip:text;">InkWell</h1>
                    </td></tr>
                    <!-- Body -->
                    <tr><td style="padding:32px 40px;">
                      <h2 style="color:#eef0f8;font-size:1.25rem;font-weight:700;margin:0 0 14px;">Welcome to InkWell! 🎉</h2>
                      <p style="color:#8b91b0;font-size:0.95rem;line-height:1.7;margin:0 0 28px;">
                        You're one click away from receiving the best stories, tutorials, and insights from the InkWell community — delivered straight to your inbox.
                      </p>
                      <a href="{confirmationLink}"
                         style="display:inline-block;background:linear-gradient(135deg,#6c63ff,#8b5cf6);color:#fff;text-decoration:none;padding:14px 32px;border-radius:10px;font-size:0.95rem;font-weight:600;letter-spacing:0.02em;">
                        ✅ Confirm My Subscription
                      </a>
                      <p style="color:#4a4f6a;font-size:0.78rem;margin:24px 0 0;line-height:1.6;">
                        If you didn't subscribe to InkWell, you can safely ignore this email.<br />This link expires in 48 hours.
                      </p>
                    </td></tr>
                    <!-- Footer -->
                    <tr><td style="padding:20px 40px;border-top:1px solid rgba(255,255,255,0.06);">
                      <p style="color:#4a4f6a;font-size:0.75rem;margin:0;text-align:center;">© 2026 InkWell · Built with ✍️</p>
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        private static string BuildNotificationHtml(
            string postTitle, string authorName, string postUrl, string unsubscribeLink) => $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>New post on InkWell</title>
            </head>
            <body style="margin:0;padding:0;background:#080b14;font-family:'Segoe UI',sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#080b14;padding:40px 0;">
                <tr><td align="center">
                  <table width="560" cellpadding="0" cellspacing="0" style="background:#131825;border-radius:16px;overflow:hidden;border:1px solid rgba(255,255,255,0.06);">
                    <!-- Header gradient bar -->
                    <tr><td style="height:4px;background:linear-gradient(90deg,#6c63ff,#a78bfa,#e879f9);"></td></tr>
                    <!-- Logo -->
                    <tr><td align="center" style="padding:36px 40px 0;">
                      <p style="font-size:2.2rem;margin:0;">✍️</p>
                      <h1 style="margin:10px 0 0;font-size:1.6rem;font-weight:800;background:linear-gradient(135deg,#6c63ff,#a78bfa);-webkit-background-clip:text;-webkit-text-fill-color:transparent;background-clip:text;">InkWell</h1>
                      <p style="color:#8b91b0;font-size:0.82rem;margin:6px 0 0;">Your weekly dose of stories &amp; insights</p>
                    </td></tr>
                    <!-- Body -->
                    <tr><td style="padding:32px 40px;">
                      <p style="color:#6c63ff;font-size:0.78rem;font-weight:700;text-transform:uppercase;letter-spacing:1px;margin:0 0 12px;">New Post</p>
                      <h2 style="color:#eef0f8;font-size:1.4rem;font-weight:800;line-height:1.3;margin:0 0 10px;">{postTitle}</h2>
                      <p style="color:#8b91b0;font-size:0.9rem;margin:0 0 28px;">by <span style="color:#a78bfa;font-weight:600;">{authorName}</span></p>
                      <a href="{postUrl}"
                         style="display:inline-block;background:linear-gradient(135deg,#6c63ff,#8b5cf6);color:#fff;text-decoration:none;padding:14px 32px;border-radius:10px;font-size:0.95rem;font-weight:600;letter-spacing:0.02em;">
                        📖 Read Now
                      </a>
                    </td></tr>
                    <!-- Footer -->
                    <tr><td style="padding:20px 40px;border-top:1px solid rgba(255,255,255,0.06);">
                      <p style="color:#4a4f6a;font-size:0.75rem;margin:0;text-align:center;">
                        © 2026 InkWell ·
                        <a href="{unsubscribeLink}" style="color:#4a4f6a;text-decoration:underline;">Unsubscribe</a>
                      </p>
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;
    }
}
