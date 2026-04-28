using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommentService.Data;
using CommentService.Models;
using CommentService.Dtos;
using System.Net.Http.Json;

namespace CommentService.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentController : ControllerBase
    {
        private readonly CommentDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CommentController(CommentDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = new Comment
            {
                PostId = request.PostId,
                AuthorId = request.AuthorId,
                AuthorName = request.AuthorName,
                Content = request.Content,
                ParentCommentId = request.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Notify Post Author
            try
            {
                var client = _httpClientFactory.CreateClient();
                // 1. Fetch Post Details to get the original author
                var postServiceUrl = _configuration["ServiceUrls:PostService"] ?? "http://localhost:5200";
                var post = await client.GetFromJsonAsync<PostResponse>($"{postServiceUrl}/api/posts/{comment.PostId}");
                
                if (post != null && post.AuthorId != comment.AuthorId) // Don't notify if commenting on own post
                {
                    var userAlert = new 
                    { 
                        UserId = post.AuthorId, 
                        Message = $"💬 {comment.AuthorName} commented on your post: '{post.Title}'", 
                        Type = "INFO" 
                    };
                    var notificationUrl = _configuration["ServiceUrls:NotificationService"] ?? "http://localhost:5700";
                    var alertRes = await client.PostAsJsonAsync($"{notificationUrl}/api/notifications", userAlert);
                    
                    if (alertRes.IsSuccessStatusCode)
                        Console.WriteLine($"[COMMENT SERVICE] Notification sent to Author {post.AuthorId}");
                    else
                        Console.WriteLine($"[COMMENT SERVICE] Failed to send notification. Status: {alertRes.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[COMMENT SERVICE] Critical failure: {ex.Message}");
            }

            return Ok(new { Message = "Comment added successfully.", Comment = comment });
        }

        private record PostResponse(int Id, string Title, int AuthorId);

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetCommentsByPostId(int postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDto request)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            if (comment.AuthorId != request.AuthorId && request.UserRole != "Admin")
            {
                return StatusCode(403, new { Message = "You are not authorized to update this comment." });
            }

            comment.Content = request.Content;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Comment updated successfully.", Comment = comment });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id, [FromQuery] int authorId, [FromQuery] string? userRole)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            if (comment.AuthorId != authorId && userRole != "Admin")
            {
                return StatusCode(403, new { Message = "You are not authorized to delete this comment." });
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Comment deleted successfully." });
        }
    }
}
