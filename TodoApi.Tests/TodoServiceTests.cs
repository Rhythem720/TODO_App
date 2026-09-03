using Moq;
using TodoApi.Models;
using TodoApi.Repositories;
using TodoApi.Services;

namespace TodoApi.Tests;

public class TodoServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidTodo_CreatesAndReturnsTodo()
    {
        var repoMock = new Mock<ITodoRepository>();
        var input = new Todo { Title = "Test", Description = "Desc" };
        var saved = new Todo { Id = 1, Title = "Test", Description = "Desc", CreatedAt = DateTime.UtcNow };

        repoMock.Setup(r => r.CreateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(saved);

        var service = new TodoService(repoMock.Object);

        var result = await service.CreateAsync(input);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        repoMock.Verify(r => r.CreateAsync(It.Is<Todo>(t => t.Title == "Test"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_MissingTitle_ThrowsArgumentException()
    {
        var repoMock = new Mock<ITodoRepository>();
        var service = new TodoService(repoMock.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new Todo { Title = " " }));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTodos()
    {
        var repoMock = new Mock<ITodoRepository>();
        var list = new List<Todo> { new Todo { Id = 1, Title = "A" }, new Todo { Id = 2, Title = "B" } };

        repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var service = new TodoService(repoMock.Object);
        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNull()
    {
        var repoMock = new Mock<ITodoRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Todo?)null);

        var service = new TodoService(repoMock.Object);
        var result = await service.UpdateAsync(999, new Todo { Title = "X" });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenRepositoryReturnsTrue_ReturnsTrue()
    {
        var repoMock = new Mock<ITodoRepository>();
        repoMock.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var service = new TodoService(repoMock.Object);
        var result = await service.DeleteAsync(1);

        Assert.True(result);
    }
}