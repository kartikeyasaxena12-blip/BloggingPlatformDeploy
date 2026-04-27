using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Models;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationDbContext _context;

        public NotificationController(NotificationDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20) // Limit to last 20 notifications
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Notification marked as read." });
        }

        // Internal method for manual testing (usually this would be called by other services)
        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] Notification request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            request.CreatedAt = DateTime.UtcNow;
            request.IsRead = false;

            Console.WriteLine($"[NOTIFICATION SERVICE] Creating notification for User {request.UserId}: {request.Message}");

            _context.Notifications.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Notification created.", Notification = request });
        }
    }
}
