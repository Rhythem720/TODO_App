using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Repositories.Infrastructure;

namespace TodoApi.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly TodoDbContext _dbcontext;

        public TodoRepository(TodoDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Todo> CreateAsync(Todo todo, CancellationToken ct = default)
        {
            todo.CreatedAt = DateTime.UtcNow;
            _dbcontext.Todos.Add(todo);
            await _dbcontext.SaveChangesAsync(ct);
            return todo;
        }

        public async Task<List<Todo>> GetAllAsync(CancellationToken ct = default)
        {
            return await _dbcontext.Todos
                            .AsNoTracking()
                            .OrderBy(t => t.Id)
                            .ToListAsync(ct);
        }

        public async Task<Todo?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _dbcontext.Todos
                            .AsNoTracking()
                            .FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<bool> UpdateAsync(Todo todo, CancellationToken ct = default)
        {
            var existing = await _dbcontext.Todos.FindAsync(new object[] { todo.Id }, ct);
            if (existing == null) return false;

            existing.Title = todo.Title;
            existing.Description = todo.Description;
            existing.IsCompleted = todo.IsCompleted;

            await _dbcontext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _dbcontext.Todos.FindAsync(new object[] { id }, ct);
            if (entity == null) return false;

            _dbcontext.Todos.Remove(entity);
            await _dbcontext.SaveChangesAsync(ct);
            return true;
        }

    }
}
