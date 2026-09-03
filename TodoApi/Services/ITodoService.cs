using TodoApi.Models;

namespace TodoApi.Services
{
        public interface ITodoService
        {
            Task<Todo> CreateAsync(Todo todo, CancellationToken ct = default);
            Task<List<Todo>> GetAllAsync(CancellationToken ct = default);
            Task<Todo?> GetByIdAsync(int id, CancellationToken ct = default);
            Task<Todo?> UpdateAsync(int id, Todo todo, CancellationToken ct = default);
            Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        }
}
