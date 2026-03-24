using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8)]
        public string Password { get; set; }
    }
}
