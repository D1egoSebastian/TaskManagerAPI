using System.ComponentModel.DataAnnotations;
using TaskManagerAPI.Models;


namespace TaskManagerAPI.DTOs

{
    public class CreateTaskDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        [Required]
        public string? Description { get; set; }
        public Models.TaskStatus Status { get; set; }
    }
}
