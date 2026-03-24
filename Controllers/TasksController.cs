using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;
using System.Security.Claims;
using TaskStatus = TaskManagerAPI.Models.TaskStatus;

namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetTasks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] TaskStatus? status = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "createdAt",
            [FromQuery] bool ascending = false)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var query = _context.Tasks.Where(x => x.UserId == userId.Value);

            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(x =>
                    x.Title.ToLower().Contains(search.ToLower()) ||
                    (x.Description != null && x.Description.ToLower().Contains(search.ToLower())));

            query = sortBy.ToLower() switch
            {
                "title" => ascending ? query.OrderBy(x => x.Title) : query.OrderByDescending(x => x.Title),
                "status" => ascending ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status),
                _ => ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var tasks = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = tasks,
                total = totalCount,
                page,
                pageSize,
                totalPages
            });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                return NotFound(new { message = "Task not found" });

            return Ok(task);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var taskExists = await _context.Tasks.AnyAsync(x =>
                x.UserId == userId.Value && x.Title.ToLower() == dto.Title.ToLower());

            if (taskExists)
                return BadRequest(new { message = "Task with this title already exists" });

            var newTask = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                UserId = userId.Value
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            return Created($"/api/tasks/{newTask.Id}", newTask);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var task = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (task == null)
                return NotFound(new { message = "Task not found" });

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [Authorize]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var task = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (task == null)
                return NotFound(new { message = "Task not found" });

            task.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var task = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (task == null)
                return NotFound(new { message = "Task not found" });

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task deleted successfully" });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;
            return int.Parse(userIdClaim.Value);
        }
    }
}
