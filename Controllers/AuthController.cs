using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var emailExists = await _context.Users.AnyAsync(x => x.Email == dto.Email);
            if (emailExists)
                return Conflict(new { message = "This email already exists" });

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var newUser = new User
            {
                Email = dto.Email,
                Name = dto.Name,
                Password = passwordHash
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Created(string.Empty, new
            {
                id = newUser.Id,
                name = newUser.Name,
                email = newUser.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized(new { message = "Invalid credentials" });

            var token = GenerateJwtToken(user);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

            return Ok(new
            {
                token,
                refreshToken = refreshToken.Token,
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email
                }
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == dto.Token);

            if (refreshToken == null)
                return Unauthorized(new { message = "Invalid refresh token" });

            if (refreshToken.IsRevoked)
                return Unauthorized(new { message = "Refresh token has been revoked" });

            if (refreshToken.Expires < DateTime.UtcNow)
                return Unauthorized(new { message = "Refresh token has expired" });

            refreshToken.IsRevoked = true;

            var newToken = GenerateJwtToken(refreshToken.User);
            var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(refreshToken.UserId);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = newToken,
                refreshToken = newRefreshToken.Token,
                user = new
                {
                    id = refreshToken.User.Id,
                    name = refreshToken.User.Name,
                    email = refreshToken.User.Email
                }
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == dto.Token);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Logged out successfully" });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresMinutes = int.Parse(_configuration["Jwt:ExpiresMinutes"] ?? "60");

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<RefreshToken> GenerateAndSaveRefreshTokenAsync(int userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = GenerateRefreshToken(),
                Expires = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
