using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Repositories.Infrastructure
{
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options)
            : base(options)
        {
        }

        public DbSet<Todo> Todos { get; set; }
    }
}
