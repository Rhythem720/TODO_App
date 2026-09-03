using Microsoft.AspNetCore.Mvc;
using TodoApi.Common;
using TodoApi.DTO;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _service;
        private readonly ILogger<TodosController> _logger;

        public TodosController(ITodoService service, ILogger<TodosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TodoCreateDTO dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var todo = new Todo
            {
                Title = dto.Title,
                Description = dto.Description,
                IsCompleted = dto.IsCompleted
            };

            try
            {
                var created = await _service.CreateAsync(todo, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, Mapper.MapToReadDto(created));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating todo");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoReadDTO>>> GetAll(CancellationToken ct = default)
        {
            var todos = await _service.GetAllAsync(ct);
            return Ok(todos.Select(Mapper.MapToReadDto));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TodoReadDTO>> GetById(int id, CancellationToken ct = default)
        {
            var todo = await _service.GetByIdAsync(id, ct);
            if (todo == null) return NotFound();
            return Ok(Mapper.MapToReadDto(todo));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TodoUpdateDTO dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var update = new Todo
            {
                Title = dto.Title,
                Description = dto.Description,
                IsCompleted = dto.IsCompleted
            };

            var updated = await _service.UpdateAsync(id, update, ct);
            if (updated == null) return NotFound();
            return Ok(Mapper.MapToReadDto(updated));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            var deleted = await _service.DeleteAsync(id, ct);
            if (!deleted) return NotFound();
            return NoContent();
        }

        
    }

}