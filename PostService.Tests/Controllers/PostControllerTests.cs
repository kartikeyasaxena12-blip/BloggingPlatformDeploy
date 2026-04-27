using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PostService.Controllers;
using PostService.Data;
using PostService.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PostService.Tests.Controllers
{
    [TestFixture]
    public class PostControllerTests
    {
        private PostDbContext _dbContext;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private PostController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<PostDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _dbContext = new PostDbContext(options);

            _dbContext.Posts.Add(new Post { Id = 1, Title = "Test Post", Content = "Test Content", AuthorId = 1, CategoryId = 1 });
            _dbContext.SaveChanges();

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();

            _controller = new PostController(_dbContext, _mockHttpClientFactory.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetAllPosts_ShouldReturnOk_WithPosts()
        {
            var result = await _controller.GetAllPosts(null, null, "latest") as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));

            var posts = result.Value as List<Post>;
            Assert.That(posts, Is.Not.Null);
            Assert.That(posts.Count, Is.EqualTo(1));
            Assert.That(posts[0].Title, Is.EqualTo("Test Post"));
        }
    }
}
