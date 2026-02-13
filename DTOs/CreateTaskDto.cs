using TaskManagerAPI.Models;


namespace TaskManagerAPI.DTOs

{
    public class CreateTaskDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public Models.TaskStatus Status { get; set; }
    }
}
