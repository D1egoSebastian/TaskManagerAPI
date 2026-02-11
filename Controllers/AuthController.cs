using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;

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
        public AuthController(AppDbContext context)
        {
            _context = context;
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
                            x = newUser.Name
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
                var user = _context.Users.FirstOrDefault( x => x.Email == dto.Email);
                if (user == null)
                {
                    return Unauthorized();
                }

                if(!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                {
                    return Unauthorized();
                }


                //Realizar lo del token

            }catch (Exception ex)
            {
                return BadRequest($"Error {ex.Message}");
            }
        }
    }
}
