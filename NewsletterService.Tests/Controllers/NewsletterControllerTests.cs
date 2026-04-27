using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using NewsletterService.Controllers;
using NewsletterService.Data;
using NewsletterService.Models;
using NewsletterService.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NewsletterService.Tests.Controllers
{
    [TestFixture]
    public class NewsletterControllerTests
    {
        private NewsletterDbContext _dbContext;
        private Mock<IEmailService> _mockEmailService;
        private Mock<IOptions<EmailSettings>> _mockOptions;
        private NewsletterController _controller;

        [SetUp]
        public void Setup()
        {
            var dbOptions = new DbContextOptionsBuilder<NewsletterDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _dbContext = new NewsletterDbContext(dbOptions);

            _dbContext.Subscribers.Add(new Subscriber { Id = 1, Email = "test@example.com", IsConfirmed = true });
            _dbContext.SaveChanges();

            _mockEmailService = new Mock<IEmailService>();
            _mockOptions = new Mock<IOptions<EmailSettings>>();
            _mockOptions.Setup(o => o.Value).Returns(new EmailSettings { ServiceBaseUrl = "http://test" });

            _controller = new NewsletterController(_dbContext, _mockEmailService.Object, _mockOptions.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetAllSubscribers_ShouldReturnOk_WithSubscribers()
        {
            var result = await _controller.GetAllSubscribers() as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));

            // The controller returns an anonymous type, but we can verify it's an enumerable
            var value = result.Value;
            Assert.That(value, Is.Not.Null);
        }
    }
}
