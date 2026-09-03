using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface ITodoRepository
    {
        Task<Todo> CreateAsync(Todo todo, CancellationToken ct = default);
        Task<List<Todo>> GetAllAsync(CancellationToken ct = default);
        Task<Todo?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<bool> UpdateAsync(Todo todo, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
