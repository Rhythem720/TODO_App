
using Microsoft.EntityFrameworkCore;
using TodoApi.Repositories;
using TodoApi.Repositories.Infrastructure;
using TodoApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure DbContext (appsettings.json: ConnectionStrings:DefaultConnection)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=todos.db";
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlServer(connectionString));

//InitializeDatabase(); Removing it since we are using EF Core migrations to handle database creation and updates.

builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

