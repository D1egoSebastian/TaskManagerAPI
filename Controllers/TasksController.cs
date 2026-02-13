using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;
using System.Security.Claims;


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

        //GET    /api/tasks
        [Authorize]
        [HttpGet]
        public IActionResult GetTasks()
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(userIdClaim.Value);


            var tasks = _context.Tasks.ToList()
                .Where(x => x.UserId == currentUserId);

            if (!tasks.Any())
            {
                return NotFound("no tasks to show.");
            }

            return Ok(tasks);
        }


        //GET    /api/tasks/{id}
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetTaskById(int id)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(userIdClaim.Value);

            var Task = _context.Tasks.FirstOrDefault(t => t.Id == id && t.UserId == currentUserId);

            if (Task == null)
            {
                return NotFound("there is no task with that id");
            }

            return Ok(Task);
        }

        //POST   /api/tasks
        [Authorize]
        [HttpPost]
        public IActionResult CreateTask([FromBody] CreateTaskDto dto)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(userIdClaim.Value);

            var newTask = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                CreatedAt = DateTime.Now
            };

            newTask.UserId = currentUserId;

            var taskexist = _context.Tasks.Any(x => x.UserId == currentUserId && x.Title == newTask.Title);

            if(taskexist)
            {
                return BadRequest("task already exist");
            }

            _context.Tasks.Add(newTask);
            _context.SaveChanges();

            return Ok(newTask);
        }

        //PUT    /api/tasks/{id}
        [Authorize]
        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, CreateTaskDto dto)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(userIdClaim.Value);

            var taskid = _context.Tasks.FirstOrDefault(x => x.Id == id && x.UserId == currentUserId);

            if(taskid == null)
            {
                return NotFound("that task dont exist.");
            }

            taskid.Title = dto.Title;
            taskid.Description = dto.Description;

            _context.SaveChanges();

            return Ok(taskid);

        }

        //DELETE /api/tasks/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(userIdClaim.Value);

            var tasktoEliminate = _context.Tasks.FirstOrDefault(x => x.Id == id && x.UserId == currentUserId);

            if (tasktoEliminate == null)
            {
                return NotFound("there is no task to delete.");
            }

            _context.Tasks.Remove(tasktoEliminate);
            _context.SaveChanges();

            return Ok();
        }

    }
}
