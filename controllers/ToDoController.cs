using CrudApi.Data;
using CrudApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrudApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TodosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Todos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTodos()
        {
            var todos = await _context.ToDos
                                      .Include(t => t.User)
                                      .ToListAsync();

            var todosDto = todos.Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.IsCompleted,
                t.Status,
                t.UserId,
                t.CreatedDate,
                User = new
                {
                    t.User.Id,
                    t.User.Username,
                    t.User.FullName,
                    t.User.Address,
                    t.User.Phone,
                    t.User.Email
                }
            });

            return Ok(todosDto);
        }

        // GET: api/Todos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTodo(int id)
        {
            var todo = await _context.ToDos
                                     .Include(t => t.User)
                                     .FirstOrDefaultAsync(t => t.Id == id);

            if (todo == null)
            {
                return NotFound();
            }

            var todoDto = new
            {
                todo.Id,
                todo.Name,
                todo.Description,
                todo.IsCompleted,
                todo.Status,
                todo.UserId,
                todo.CreatedDate,
                User = new
                {
                    todo.User.Id,
                    todo.User.Username,
                    todo.User.FullName,
                    todo.User.Address,
                    todo.User.Phone,
                    todo.User.Email
                }
            };

            return Ok(todoDto);
        }


        // PUT: api/Todos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodo(int id, TodoInputModel updatedTodo)
        {
            if (id != updatedTodo.Id)
            {
                return BadRequest();
            }

            var existingTodo = await _context.ToDos.FindAsync(id);

            if (existingTodo == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(updatedTodo.UserId);

            if (user == null)
            {
                return BadRequest("Invalid UserId");
            }

            existingTodo.Name = updatedTodo.Name;
            existingTodo.Description = updatedTodo.Description;
            existingTodo.IsCompleted = updatedTodo.IsCompleted;
            existingTodo.Status = updatedTodo.Status;
            existingTodo.UserId = updatedTodo.UserId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Todos
        [HttpPost]
        public async Task<ActionResult<Todo>> PostTodo(TodoInputModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);

            if (user == null)
            {
                return BadRequest("Invalid UserId");
            }

            var newTodo = new Todo
            {
                Name = model.Name,
                Description = model.Description,
                IsCompleted = model.IsCompleted,
                Status = model.Status,
                UserId = model.UserId,
                CreatedDate = DateTime.UtcNow,
                User = user
            };

            _context.ToDos.Add(newTodo);
            await _context.SaveChangesAsync();

            var responseTodo = new
            {
                newTodo.Id,
                newTodo.Name,
                newTodo.Description,
                newTodo.IsCompleted,
                newTodo.Status,
                newTodo.UserId,
                CreatedDate = newTodo.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            return CreatedAtAction(nameof(GetTodo), new { id = newTodo.Id }, responseTodo);
        }

        // DELETE: api/Todos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var Todo = await _context.ToDos.FindAsync(id);
            if (Todo == null)
            {
                return NotFound();
            }

            _context.ToDos.Remove(Todo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Todos/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchTodos(DateTime? fromDate, int? userId)
        {
            IQueryable<Todo> query = _context.ToDos.Include(t => t.User);

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.CreatedDate.Date == fromDate.Value.Date);
            }

            if (userId.HasValue)
            {
                query = query.Where(t => t.UserId == userId);
            }

            var todos = await query.ToListAsync();

            var todosDto = todos.Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.IsCompleted,
                t.Status,
                t.UserId,
                t.CreatedDate,
                User = new
                {
                    t.User.Id,
                    t.User.Username,
                    t.User.FullName,
                    t.User.Address,
                    t.User.Phone,
                    t.User.Email
                }
            });

            return Ok(todosDto);
        }

        [HttpGet("sortBy")]
        public async Task<ActionResult<IEnumerable<object>>> GetTodos(string sortBy)
        {
            IQueryable<Todo> query = _context.ToDos.Include(t => t.User);

            switch (sortBy)
            {
                case "date":
                    query = query.OrderBy(t => t.CreatedDate);
                    break;
                case "user":
                    query = query.OrderBy(t => t.User.Username);
                    break;
                default:
                    query = query.OrderBy(t => t.CreatedDate);
                    break;
            }

            var todos = await query.ToListAsync();

            var todosDto = todos.Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.IsCompleted,
                t.Status,
                t.UserId,
                t.CreatedDate,
                User = new
                {
                    t.User.Id,
                    t.User.Username,
                    t.User.FullName,
                    t.User.Address,
                    t.User.Phone,
                    t.User.Email
                }
            });

            return Ok(todosDto);
        }

        private bool TodoExists(int id)
        {
            return _context.ToDos.Any(e => e.Id == id);
        }
    }
}
