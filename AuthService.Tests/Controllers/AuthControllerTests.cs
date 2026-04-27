using AuthService.Controllers;
using AuthService.Data;
using AuthService.Dtos;
using AuthService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthService.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        // These are the dependencies our controller needs to work
        private AuthDbContext _dbContext;
        private Mock<IConfiguration> _mockConfiguration;
        private AuthController _controller;

        /// <summary>
        /// This SetUp method runs BEFORE every single test.
        /// It gives us a fresh, clean database and controller so tests don't interfere with each other.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // 1. Create a fake, temporary "In-Memory" database for testing.
            // This is much faster than a real SQL database and prevents us from accidentally modifying real data.
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AuthDbContext(options);

            // 2. We need to "Mock" (fake) the configuration (appsettings.json).
            // The AuthController needs JWT secret keys to generate a token, so we provide fake keys here.
            _mockConfiguration = new Mock<IConfiguration>();
            var mockJwtSection = new Mock<IConfigurationSection>();
            mockJwtSection.Setup(x => x["Key"]).Returns("SuperSecretKeyThatIsAtLeast32BytesLong!");
            mockJwtSection.Setup(x => x["ExpireMinutes"]).Returns("60");
            mockJwtSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
            mockJwtSection.Setup(x => x["Audience"]).Returns("TestAudience");
            
            _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(mockJwtSection.Object);

            // 3. Finally, create the controller using our fake database and fake configuration
            _controller = new AuthController(_dbContext, _mockConfiguration.Object);
        }

        /// <summary>
        /// This TearDown method runs AFTER every single test.
        /// It cleans up the temporary database to free up memory.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Register_ShouldReturnOk_WhenUserIsNew()
        {
            // ARRANGE: Set up the data we need for the test
            var request = new RegisterRequest
            {
                Username = "testuser",
                Email = "testuser@example.com",
                Password = "Password123",
                FullName = "Test User"
            };

            // ACT: Execute the method we are testing
            var result = await _controller.Register(request) as OkObjectResult;

            // ASSERT: Verify the result is exactly what we expect
            Assert.That(result, Is.Not.Null, "The result should not be null.");
            Assert.That(result.StatusCode, Is.EqualTo(200), "The HTTP status code should be 200 OK.");

            // Also verify that the user was actually saved into our database
            var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            Assert.That(userInDb, Is.Not.Null, "The user should exist in the database.");
            Assert.That(userInDb.Username, Is.EqualTo("testuser"), "The username should match the registration request.");
            Assert.That(userInDb.Role, Is.EqualTo("Reader"), "New users should default to the 'Reader' role.");
        }

        [Test]
        public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            // ARRANGE: Add a user to the database BEFORE we try to register
            _dbContext.Users.Add(new User 
            { 
                Username = "existinguser", 
                Email = "duplicate@example.com", 
                PasswordHash = "hash", 
                FullName = "Existing User",
                Role = "Reader"
            });
            await _dbContext.SaveChangesAsync();

            // Try to register a new user using the EXACT SAME email
            var request = new RegisterRequest
            {
                Username = "newuser",
                Email = "duplicate@example.com", // This email is already taken!
                Password = "Password123",
                FullName = "New User"
            };

            // ACT: Execute the registration
            var result = await _controller.Register(request) as BadRequestObjectResult;

            // ASSERT: The system should block it and return a 400 Bad Request
            Assert.That(result, Is.Not.Null, "The result should not be null.");
            Assert.That(result.StatusCode, Is.EqualTo(400), "The HTTP status code should be 400 Bad Request.");
        }

        [Test]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
        {
            // ARRANGE: Set up a request with an email that doesn't exist (or wrong password)
            var request = new LoginRequest
            {
                Email = "nobody@example.com",
                Password = "WrongPassword123"
            };

            // ACT: Try to log in
            var result = await _controller.Login(request) as UnauthorizedObjectResult;

            // ASSERT: The system should deny access and return 401 Unauthorized
            Assert.That(result, Is.Not.Null, "The result should not be null.");
            Assert.That(result.StatusCode, Is.EqualTo(401), "The HTTP status code should be 401 Unauthorized.");
        }

        [Test]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreCorrect()
        {
            // ARRANGE: First, we need to add a valid user to our database with a properly hashed password
            string validPassword = "MySecretPassword123!";
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(validPassword);

            _dbContext.Users.Add(new User 
            { 
                Username = "validuser", 
                Email = "valid@example.com", 
                PasswordHash = hashedPassword, // Store the hashed version, not the plain text
                FullName = "Valid User",
                Role = "Reader"
            });
            await _dbContext.SaveChangesAsync();

            // Create a login request matching the user we just created
            var request = new LoginRequest
            {
                Email = "valid@example.com",
                Password = validPassword // Provide the plain text password
            };

            // ACT: Try to log in
            var result = await _controller.Login(request) as OkObjectResult;

            // ASSERT: The login should succeed (200 OK)
            Assert.That(result, Is.Not.Null, "The result should not be null.");
            Assert.That(result.StatusCode, Is.EqualTo(200), "The HTTP status code should be 200 OK.");
            
            // Extract the generated JWT token from the response
            var authResponse = result.Value as AuthResponse;
            Assert.That(authResponse, Is.Not.Null, "The authentication response should contain data.");
            Assert.That(authResponse.Token, Is.Not.Empty, "A JWT token should have been generated.");
            Assert.That(authResponse.Email, Is.EqualTo("valid@example.com"), "The response should echo back the user's email.");
        }
    }
}
