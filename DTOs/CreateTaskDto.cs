namespace TaskManagerAPI.DTOs
{
    public class CreateTaskDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
    }
}
