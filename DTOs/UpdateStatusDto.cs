using TaskStatus = TaskManagerAPI.Models.TaskStatus;

namespace TaskManagerAPI.DTOs
{
    public class UpdateStatusDto
    {
        public TaskStatus Status { get; set; }
    }
}
