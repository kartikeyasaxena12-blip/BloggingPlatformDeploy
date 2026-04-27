using System.ComponentModel.DataAnnotations;

namespace CategoryService.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
    }
}
