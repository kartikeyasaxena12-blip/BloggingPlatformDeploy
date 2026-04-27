using System.ComponentModel.DataAnnotations;

namespace PostService.Dtos
{
    public class UpdatePostDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int AuthorId { get; set; }

        public string? UserRole { get; set; }

        public int? CategoryId { get; set; }
    }
}
