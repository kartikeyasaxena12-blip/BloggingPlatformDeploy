using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CategoryService.Data;
using CategoryService.Models;
using CategoryService.Dtos;

namespace CategoryService.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryDbContext _context;

        public CategoryController(CategoryDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = new Category
            {
                Name = request.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Category created successfully", Category = category });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }
    }
}
