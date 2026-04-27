using CategoryService.Controllers;
using CategoryService.Data;
using CategoryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CategoryService.Tests.Controllers
{
    [TestFixture]
    public class CategoryControllerTests
    {
        private CategoryDbContext _dbContext;
        private CategoryController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CategoryDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _dbContext = new CategoryDbContext(options);

            _dbContext.Categories.Add(new Category { Id = 1, Name = "Technology" });
            _dbContext.SaveChanges();

            _controller = new CategoryController(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetAllCategories_ShouldReturnOk_WithCategories()
        {
            var result = await _controller.GetAllCategories() as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));

            var categories = result.Value as List<Category>;
            Assert.That(categories, Is.Not.Null);
            Assert.That(categories.Count, Is.EqualTo(1));
            Assert.That(categories[0].Name, Is.EqualTo("Technology"));
        }
    }
}
