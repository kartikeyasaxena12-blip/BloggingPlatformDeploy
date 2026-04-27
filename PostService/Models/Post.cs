using System.ComponentModel.DataAnnotations;

namespace PostService.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public int ViewCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
