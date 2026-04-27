using System.ComponentModel.DataAnnotations;

namespace CommentService.Dtos
{
    public class CreateCommentDto
    {
        public int PostId { get; set; }

        public int AuthorId { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? ParentCommentId { get; set; }
    }
}
