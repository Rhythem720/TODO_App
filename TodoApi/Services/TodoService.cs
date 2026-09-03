
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _repository;

        public TodoService(ITodoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Todo> CreateAsync(Todo todo, CancellationToken ct = default)
        {
            if (todo is null) throw new ArgumentNullException(nameof(todo));
            if (string.IsNullOrWhiteSpace(todo.Title))
                throw new ArgumentException("Title is required", nameof(todo.Title));

            return await _repository.CreateAsync(todo, ct);
        }

        public async Task<List<Todo>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repository.GetAllAsync(ct);
        }

        public async Task<Todo?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0) return null;
            return await _repository.GetByIdAsync(id, ct);
        }

        public async Task<Todo?> UpdateAsync(int id, Todo todo, CancellationToken ct = default)
        {
            if (todo is null) throw new ArgumentNullException(nameof(todo));
            if (id <= 0) return null;

            var existing = await _repository.GetByIdAsync(id, ct);
            if (existing == null) return null;

            existing.Title = todo.Title;
            existing.Description = todo.Description;
            existing.IsCompleted = todo.IsCompleted;

            var updated = await _repository.UpdateAsync(existing, ct);
            return updated ? existing : null;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0) return false;
            return await _repository.DeleteAsync(id, ct);
        }

    }
}
