using System.ComponentModel.DataAnnotations;

namespace CommentService.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? ParentCommentId { get; set; } // For replies

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
