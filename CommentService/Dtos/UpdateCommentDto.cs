using System.ComponentModel.DataAnnotations;

namespace CommentService.Dtos
{
    public class UpdateCommentDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int AuthorId { get; set; }

        public string? UserRole { get; set; }
    }
}
