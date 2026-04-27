using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Data;
using AuthService.Dtos;
using AuthService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Google.Apis.Auth;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { Message = "Email already in use." });
            }

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(new { Message = "Username already taken." });
            }

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                Role = request.Email.Equals("kartikeyasaxena12@gmail.com", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Reader"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Self-Seeding Admin Account: Create it if it doesn't exist
            if (request.Email.Equals("kartikeyasaxena12@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Email == "kartikeyasaxena12@gmail.com");
                if (existingAdmin == null && request.Password == "admin123")
                {
                    var adminUser = new User
                    {
                        Username = "platform_admin",
                        Email = "kartikeyasaxena12@gmail.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        FullName = "Platform Administrator",
                        Role = "Admin",
                        IsEmailVerified = true
                    };
                    _context.Users.Add(adminUser);
                    await _context.SaveChangesAsync();
                }
            }

            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            // Verify password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            // Ensure specific user is Admin
            if (user.Email.Equals("kartikeyasaxena12@gmail.com", StringComparison.OrdinalIgnoreCase) && user.Role != "Admin")
            {
                user.Role = "Admin";
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            // Generate JWT
            var token = GenerateJwtToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl
            });
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _configuration["Google:ClientId"]! }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.TokenId, settings);

                // Find or create user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user == null)
                {
                    // Generate a unique username from email prefix, appending suffix if taken
                    var baseUsername = payload.Email.Split('@')[0];
                    var username = baseUsername;
                    if (await _context.Users.AnyAsync(u => u.Username == username))
                    {
                        username = $"{baseUsername}{new Random().Next(1000, 9999)}";
                    }

                    user = new User
                    {
                        Username = username,
                        Email = payload.Email,
                        PasswordHash = "GOOGLE_AUTH_EXTERNAL", // Placeholder — Google users cannot login with password
                        FullName = payload.Name,
                        AvatarUrl = payload.Picture,
                        IsEmailVerified = true,
                        Role = payload.Email.Equals("kartikeyasaxena12@gmail.com", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Reader"
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                var token = GenerateJwtToken(user);

                return Ok(new AuthResponse
                {
                    Token = token,
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    FullName = user.FullName,
                    AvatarUrl = user.AvatarUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Google authentication failed.", Details = ex.Message });
            }
        }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.FullName,
                user.Bio,
                user.Role,
                user.IsEmailVerified,
                user.AvatarUrl,
                user.CreatedAt
            });
        }

        [HttpPut("profile/{id}/bio")]
        public async Task<IActionResult> UpdateBio(int id, [FromBody] UpdateBioRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            user.Bio = request.Bio;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Bio updated successfully.", Bio = user.Bio });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }
    }
}
