using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;


namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /*
        POST /api/auth/register
        POST /api/auth/login
        */

        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

        }

        [HttpPost("register")]

        public IActionResult RegisterAsync(RegisterDto dto)
        {
            try
            {
                var emailExist = _context.Users.Any( x => x.Email == dto.Email);

                if (emailExist)
                {
                    return Conflict("This email already exist");
                }

                var passwordhash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                var newUser = new User
                {
                    Email = dto.Email,
                    Name = dto.Name,
                    Password = passwordhash

                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                return CreatedAtAction(
                        "", new
                        {
                            x = newUser.Name,
                            y = newUser.Email,
                        }, newUser
                    );

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


            
        }

        [HttpPost("login")]
        public IActionResult LoginAsync(LoginDto dto)
        {
            try
            {

                if(dto.Email == null || dto.Password == null)
                {
                    return BadRequest();
                }

                var user = _context.Users.FirstOrDefault( x => x.Email == dto.Email);
                if (user == null)
                {
                    return Unauthorized();
                }

                if(!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                {
                    return Unauthorized();
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name)
                };

                //inyectar IConfiguration 
                var key = _configuration["Jwt:Key"];
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var expiresMinutes = int.Parse(_configuration["Jwt:ExpiresMinutes"] ?? "60");

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var Token = new JwtSecurityToken(
                        issuer: issuer,
                        audience: audience,
                        claims: claims,
                        expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                        signingCredentials: credentials
                    );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(Token);

                return Ok(new
                {
                    token = tokenString,
                    user = new
                    {
                        id = user.Id,
                        name = user.Name,
                        email = user.Email,
                    }
                }

                 );

                

            }catch (Exception ex)
            {
                return BadRequest($"Error {ex.Message}");
            }
        }
    }
}
