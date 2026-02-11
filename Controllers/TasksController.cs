using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Data;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        /*
            
            
            
            
            

        */

        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        //GET    /api/tasks
        [HttpGet]
        public IActionResult GetTasks()
        {
            var tasks = _context.Tasks.ToList();

            if (!tasks.Any())
            {
                return BadRequest("no tasks to show.");
            }

            return Ok(tasks);
        }


        //GET    /api/tasks/{id}
        [HttpGet("{id}")]
        public IActionResult GetTaskById(int id)
        {
            var Task = _context.Tasks.FirstOrDefault(t => t.Id == id);

            if (Task == null)
            {
                return BadRequest("there is no task with that id");
            }

            return Ok(Task);
        }

        //POST   /api/tasks
        [HttpPost]
        public IActionResult CreateTask([FromBody] CreateTaskDto dto)
        {
            var newTask = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                CreatedAt = DateTime.Now
            };

            var taskexist = _context.Tasks.Any(x => x.Title == newTask.Title);

            if(taskexist)
            {
                return BadRequest("task already exist");
            }

            _context.Tasks.Add(newTask);
            _context.SaveChanges();

            return Ok(newTask);
        }

        //PUT    /api/tasks/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, CreateTaskDto dto)
        {
            var taskid = _context.Tasks.FirstOrDefault(x => x.Id == id);

            if(taskid == null)
            {
                return BadRequest("that task dont exist.");
            }

            taskid.Title = dto.Title;
            taskid.Description = dto.Description;

            _context.SaveChanges();

            return Ok(taskid);

        }

        //DELETE /api/tasks/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            var tasktoEliminate = _context.Tasks.FirstOrDefault(x => x.Id == id);

            if (tasktoEliminate == null)
            {
                return BadRequest("there is no task to delete.");
            }

            _context.Tasks.Remove(tasktoEliminate);
            _context.SaveChanges();

            return Ok();
        }

    }
}
