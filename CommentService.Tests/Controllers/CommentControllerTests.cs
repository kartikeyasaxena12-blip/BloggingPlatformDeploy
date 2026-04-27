using CommentService.Controllers;
using CommentService.Data;
using CommentService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CommentService.Tests.Controllers
{
    [TestFixture]
    public class CommentControllerTests
    {
        private CommentDbContext _dbContext;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private CommentController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<CommentDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _dbContext = new CommentDbContext(options);

            _dbContext.Comments.Add(new Comment { Id = 1, PostId = 10, AuthorId = 1, Content = "Great post!" });
            _dbContext.SaveChanges();

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();

            _controller = new CommentController(_dbContext, _mockHttpClientFactory.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetCommentsByPostId_ShouldReturnOk_WithComments()
        {
            var result = await _controller.GetCommentsByPostId(10) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));

            var comments = result.Value as List<Comment>;
            Assert.That(comments, Is.Not.Null);
            Assert.That(comments.Count, Is.EqualTo(1));
            Assert.That(comments[0].Content, Is.EqualTo("Great post!"));
        }
    }
}
