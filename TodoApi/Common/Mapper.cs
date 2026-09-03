using TodoApi.DTO;
using TodoApi.Models;

namespace TodoApi.Common
{
    public static class Mapper
    {
        public static TodoReadDTO MapToReadDto(Todo t) =>
            new TodoReadDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt
            };
    }
}
