using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Controllers;
using NotificationService.Data;
using NotificationService.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotificationService.Tests.Controllers
{
    [TestFixture]
    public class NotificationControllerTests
    {
        private NotificationDbContext _dbContext;
        private NotificationController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<NotificationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _dbContext = new NotificationDbContext(options);

            _dbContext.Notifications.Add(new Notification { Id = 1, UserId = 1, Message = "Test notification" });
            _dbContext.SaveChanges();

            _controller = new NotificationController(_dbContext);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetNotifications_ShouldReturnOk_WithNotifications()
        {
            var result = await _controller.GetNotifications(1) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));

            var notifications = result.Value as List<Notification>;
            Assert.That(notifications, Is.Not.Null);
            Assert.That(notifications.Count, Is.EqualTo(1));
            Assert.That(notifications[0].Message, Is.EqualTo("Test notification"));
        }
    }
}
