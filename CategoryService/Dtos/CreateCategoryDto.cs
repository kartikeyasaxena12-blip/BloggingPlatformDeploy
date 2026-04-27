using System.ComponentModel.DataAnnotations;

namespace CategoryService.Dtos
{
    public class CreateCategoryDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
