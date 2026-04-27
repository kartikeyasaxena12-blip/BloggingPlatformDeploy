using System.ComponentModel.DataAnnotations;

namespace PostService.Dtos
{
    public class CreatePostDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
    }
}
