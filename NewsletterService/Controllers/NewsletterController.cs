using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsletterService.Data;
using NewsletterService.Dtos;
using NewsletterService.Models;
using NewsletterService.Services;

namespace NewsletterService.Controllers
{
    [ApiController]
    [Route("api/newsletter")]
    public class NewsletterController : ControllerBase
    {
        private readonly NewsletterDbContext _context;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public NewsletterController(
            NewsletterDbContext context,
            IEmailService emailService,
            Microsoft.Extensions.Options.IOptions<EmailSettings> emailSettingsOptions)
        {
            _context = context;
            _emailService = emailService;
            _emailSettings = emailSettingsOptions.Value;
        }

        // ── POST /api/newsletter/subscribe ────────────────────────────────────
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Subscribers.AnyAsync(s => s.Email == request.Email))
                return BadRequest(new { Message = "This email is already subscribed." });

            var subscriber = new Subscriber
            {
                Email             = request.Email,
                ConfirmationToken = Guid.NewGuid().ToString("N"),
                UnsubscribeToken  = Guid.NewGuid().ToString("N"),
                IsConfirmed       = false,
                SubscribedAt      = DateTime.UtcNow
            };

            _context.Subscribers.Add(subscriber);
            await _context.SaveChangesAsync();

            var confirmLink = $"{_emailSettings.ServiceBaseUrl}/api/newsletter/confirm?token={subscriber.ConfirmationToken}";
            await _emailService.SendConfirmationEmailAsync(request.Email, confirmLink);

            return Ok(new
            {
                Message = "Almost there! We sent a confirmation email — please check your inbox to complete your subscription."
            });
        }

        // ── GET /api/newsletter/confirm?token= ────────────────────────────────
        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Invalid confirmation token.");

            var subscriber = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.ConfirmationToken == token);

            if (subscriber == null)
                return Content(HtmlHelper.ResultPage("❌ Token Not Found",
                    "This confirmation link is invalid or has already been used.", false), "text/html");

            if (subscriber.IsConfirmed)
                return Content(HtmlHelper.ResultPage("✅ Already Confirmed",
                    "Your subscription was already confirmed. You're all set!", true), "text/html");

            subscriber.IsConfirmed = true;
            await _context.SaveChangesAsync();

            return Content(HtmlHelper.ResultPage("🎉 Subscription Confirmed!",
                "Welcome to InkWell! You'll receive updates whenever a new post is published.", true), "text/html");
        }

        // ── GET /api/newsletter/unsubscribe?token= ────────────────────────────
        [HttpGet("unsubscribe")]
        public async Task<IActionResult> UnsubscribeByToken([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Invalid unsubscribe token.");

            var subscriber = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.UnsubscribeToken == token);

            if (subscriber == null)
                return Content(HtmlHelper.ResultPage("❌ Not Found",
                    "This unsubscribe link is invalid or you have already unsubscribed.", false), "text/html");

            _context.Subscribers.Remove(subscriber);
            await _context.SaveChangesAsync();

            return Content(HtmlHelper.ResultPage("👋 Unsubscribed",
                "You have been successfully removed from InkWell updates. We're sorry to see you go!", true), "text/html");
        }

        // ── POST /api/newsletter/unsubscribe (by email body) ──────────────────
        [HttpPost("unsubscribe")]
        public async Task<IActionResult> UnsubscribeByEmail([FromBody] UnsubscribeRequest request)
        {
            var subscriber = await _context.Subscribers
                .FirstOrDefaultAsync(s => s.Email == request.Email);

            if (subscriber == null)
                return NotFound(new { Message = "Email not found in our subscriber list." });

            _context.Subscribers.Remove(subscriber);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "You have been unsubscribed successfully." });
        }

        // ── GET /api/newsletter (list all subscribers) ────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAllSubscribers()
        {
            var subscribers = await _context.Subscribers
                .Select(s => new { s.Id, s.Email, s.IsConfirmed, s.SubscribedAt })
                .ToListAsync();
            return Ok(subscribers);
        }

        // ── POST /api/newsletter/admin-confirm/{id} (admin force-confirm) ─────
        [HttpPost("admin-confirm/{id}")]
        public async Task<IActionResult> AdminForceConfirm(int id)
        {
            var subscriber = await _context.Subscribers.FindAsync(id);
            if (subscriber == null)
                return NotFound(new { Message = "Subscriber not found." });

            subscriber.IsConfirmed = true;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"{subscriber.Email} confirmed successfully." });
        }

        // ── GET /api/newsletter/stats ─────────────────────────────────────────
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var total     = await _context.Subscribers.CountAsync();
            var confirmed = await _context.Subscribers.CountAsync(s => s.IsConfirmed);
            return Ok(new { Total = total, Confirmed = confirmed, Pending = total - confirmed });
        }

        // ── POST /api/newsletter/notify ───────────────────────────────────────
        [HttpPost("notify")]
        public async Task<IActionResult> NotifySubscribers([FromBody] NewPostNotificationRequest request)
        {
            var confirmed = await _context.Subscribers
                .Where(s => s.IsConfirmed)
                .ToListAsync();

            if (confirmed.Count == 0)
                return Ok(new { Message = "No confirmed subscribers to notify.", SentCount = 0 });

            int sent = 0;
            try
            {
                foreach (var sub in confirmed)
                {
                    var unsubLink = $"{_emailSettings.ServiceBaseUrl}/api/newsletter/unsubscribe?token={sub.UnsubscribeToken}";
                    await _emailService.SendNewPostNotificationAsync(
                        sub.Email, unsubLink, request.PostTitle, request.AuthorName, request.PostUrl);
                    sent++;
                }

                return Ok(new { Message = $"Notification dispatched to {sent} subscriber(s).", SentCount = sent });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"SMTP Error: {ex.Message}" });
            }
        }
    }

    // ── Supporting DTOs ───────────────────────────────────────────────────────
    public class UnsubscribeRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class NewPostNotificationRequest
    {
        public string PostTitle  { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string PostUrl    { get; set; } = string.Empty;
    }

    // ── HTML Result Page Helper ───────────────────────────────────────────────
    internal static class HtmlHelper
    {
        internal static string ResultPage(string heading, string body, bool success)
        {
            var color      = success ? "#22d3a0" : "#ff4d6d";
            var bgColor    = success ? "34,211,160" : "255,77,109";
            var statusText = success ? "Success" : "Error";
            var icon       = success ? "✅" : "❌";

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width,initial-scale=1""/>
  <title>{heading} — InkWell</title>
  <style>
    body{{margin:0;padding:0;background:#080b14;font-family:'Segoe UI',sans-serif;display:flex;align-items:center;justify-content:center;min-height:100vh;}}
    .card{{background:#131825;border:1px solid rgba(255,255,255,0.06);border-radius:20px;padding:52px 48px;text-align:center;max-width:480px;width:90%;box-shadow:0 8px 32px rgba(0,0,0,0.5);position:relative;overflow:hidden;}}
    .card::before{{content:'';position:absolute;top:0;left:0;right:0;height:4px;background:linear-gradient(90deg,#6c63ff,#a78bfa,#e879f9);}}
    .icon{{font-size:3.5rem;margin-bottom:20px;}}
    h1{{color:#eef0f8;font-size:1.5rem;font-weight:800;margin:0 0 14px;}}
    p{{color:#8b91b0;font-size:0.95rem;line-height:1.7;margin:0 0 28px;}}
    a{{display:inline-block;background:linear-gradient(135deg,#6c63ff,#8b5cf6);color:#fff;text-decoration:none;padding:13px 30px;border-radius:10px;font-size:0.9rem;font-weight:600;}}
    .status{{display:inline-block;background:rgba({bgColor},0.1);color:{color};font-size:0.75rem;font-weight:700;text-transform:uppercase;letter-spacing:1px;padding:4px 12px;border-radius:20px;margin-bottom:18px;}}
  </style>
</head>
<body>
  <div class=""card"">
    <div class=""icon"">{icon}</div>
    <div class=""status"">{statusText}</div>
    <h1>{heading}</h1>
    <p>{body}</p>
    <a href=""{_emailSettings.FrontendUrl}"">← Back to InkWell</a>
  </div>
</body>
</html>";
        }
    }
}
