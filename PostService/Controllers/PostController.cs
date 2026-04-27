using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostService.Data;
using PostService.Models;
using PostService.Dtos;
using System.Net.Http.Json;

namespace PostService.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostController : ControllerBase
    {
        private readonly PostDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public PostController(PostDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = new Post
            {
                Title = request.Title,
                Content = request.Content,
                AuthorId = request.AuthorId,
                AuthorName = request.AuthorName,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // 1. Trigger Newsletter Notification
            try
            {
                var client = _httpClientFactory.CreateClient();
                var newsNotification = new { PostTitle = post.Title, AuthorName = post.AuthorName };
                await client.PostAsJsonAsync("http://localhost:5600/api/newsletter/notify", newsNotification);
                Console.WriteLine("[POST SERVICE] Newsletter notification triggered.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[POST SERVICE] Newsletter Alert failed: {ex.Message}");
            }

            // 2. Trigger User Alert
            try
            {
                var client = _httpClientFactory.CreateClient();
                var userAlert = new { UserId = post.AuthorId, Message = $"🚀 Your post '{post.Title}' is now live!", Type = "SUCCESS" };
                var alertRes = await client.PostAsJsonAsync("http://localhost:5700/api/notifications", userAlert);
                
                if (alertRes.IsSuccessStatusCode)
                    Console.WriteLine($"[POST SERVICE] User alert sent successfully to User {post.AuthorId}");
                else
                    Console.WriteLine($"[POST SERVICE] Failed to send user alert. Status: {alertRes.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[POST SERVICE] User Alert failed: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts([FromQuery] int? categoryId, [FromQuery] string? search, [FromQuery] string? sortBy = "latest")
        {
            var query = _context.Posts.AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
            }

            query = sortBy?.ToLower() switch
            {
                "oldest" => query.OrderBy(p => p.CreatedAt),
                "popular" => query.OrderByDescending(p => p.ViewCount),
                "most_liked" => query.OrderByDescending(p => p.LikeCount),
                _ => query.OrderByDescending(p => p.CreatedAt) // "latest" is default
            };

            var posts = await query.ToListAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound(new { Message = "Post not found." });
            }

            // Increment view count
            post.ViewCount++;
            _context.Update(post);
            await _context.SaveChangesAsync();

            return Ok(post);
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikePost(int id, [FromQuery] int userId)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == id && l.UserId == userId);

            if (existingLike == null)
            {
                var like = new PostLike { PostId = id, UserId = userId };
                _context.PostLikes.Add(like);
                post.LikeCount++;
                await _context.SaveChangesAsync();
            }

            return Ok(new { LikeCount = post.LikeCount });
        }

        [HttpDelete("{id}/like")]
        public async Task<IActionResult> UnlikePost(int id, [FromQuery] int userId)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var like = await _context.PostLikes
                .FirstOrDefaultAsync(l => l.PostId == id && l.UserId == userId);

            if (like != null)
            {
                _context.PostLikes.Remove(like);
                post.LikeCount = Math.Max(0, post.LikeCount - 1);
                await _context.SaveChangesAsync();
            }

            return Ok(new { LikeCount = post.LikeCount });
        }

        [HttpGet("saved")]
        public async Task<IActionResult> GetSavedPosts([FromQuery] int userId)
        {
            var savedPosts = await _context.SavedPosts
                .Where(s => s.UserId == userId)
                .Include(s => s.Post)
                .OrderByDescending(s => s.SavedAt)
                .Select(s => s.Post)
                .ToListAsync();

            return Ok(savedPosts);
        }

        [HttpPost("{id}/save")]
        public async Task<IActionResult> SavePost(int id, [FromQuery] int userId)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound(new { Message = "Post not found." });

            var existingSave = await _context.SavedPosts
                .FirstOrDefaultAsync(s => s.PostId == id && s.UserId == userId);

            if (existingSave == null)
            {
                var savedPost = new SavedPost { PostId = id, UserId = userId };
                _context.SavedPosts.Add(savedPost);
                await _context.SaveChangesAsync();
            }

            return Ok(new { Message = "Post saved successfully." });
        }

        [HttpDelete("{id}/save")]
        public async Task<IActionResult> UnsavePost(int id, [FromQuery] int userId)
        {
            var savedPost = await _context.SavedPosts
                .FirstOrDefaultAsync(s => s.PostId == id && s.UserId == userId);

            if (savedPost != null)
            {
                _context.SavedPosts.Remove(savedPost);
                await _context.SaveChangesAsync();
            }

            return Ok(new { Message = "Post unsaved successfully." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound(new { Message = "Post not found." });
            }

            if (post.AuthorId != request.AuthorId && request.UserRole != "Admin")
            {
                return StatusCode(403, new { Message = "You are not authorized to update this post." });
            }

            post.Title = request.Title;
            post.Content = request.Content;
            post.CategoryId = request.CategoryId;

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Post updated successfully.", Post = post });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id, [FromQuery] int authorId, [FromQuery] string? userRole)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound(new { Message = "Post not found." });
            }

            if (post.AuthorId != authorId && userRole != "Admin")
            {
                return StatusCode(403, new { Message = "You are not authorized to delete this post." });
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            // Trigger User Alert
            try
            {
                var client = _httpClientFactory.CreateClient();
                var userAlert = new { UserId = post.AuthorId, Message = $"🗑️ Your post '{post.Title}' has been deleted.", Type = "INFO" };
                await client.PostAsJsonAsync("http://localhost:5700/api/notifications", userAlert);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[POST SERVICE] Failed to send deletion alert: {ex.Message}");
            }

            return Ok(new { Message = "Post deleted successfully." });
        }
    }
}
