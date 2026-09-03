using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TodoApi.Controllers;
using TodoApi.DTO;
using TodoApi.Models;
using TodoApi.Services;
using Xunit;

namespace TodoApi.Tests;

public class TodosControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var serviceMock = new Mock<ITodoService>();
        var loggerMock = new Mock<ILogger<TodosController>>();
        var dto = new TodoCreateDTO { Title = "t1", Description = "d1" };
        var created = new Todo { Id = 5, Title = "t1", Description = "d1", CreatedAt = DateTime.UtcNow };

        serviceMock.Setup(s => s.CreateAsync(It.IsAny<Todo>(), default)).ReturnsAsync(created);

        var controller = new TodosController(serviceMock.Object, loggerMock.Object);
        var result = await controller.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var value = Assert.IsType<TodoReadDTO>(createdResult.Value!);
        Assert.Equal(5, value.Id);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithItems()
    {
        var serviceMock = new Mock<ITodoService>();
        var loggerMock = new Mock<ILogger<TodosController>>();

        var items = new List<Todo> { new Todo { Id = 1, Title = "A" } };
        serviceMock.Setup(s => s.GetAllAsync(default)).ReturnsAsync(items);

        var controller = new TodosController(serviceMock.Object, loggerMock.Object);
        var actionResult = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returned = Assert.IsAssignableFrom<IEnumerable<TodoReadDTO>>(ok.Value);
        Assert.Single(returned);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        var serviceMock = new Mock<ITodoService>();
        var loggerMock = new Mock<ILogger<TodosController>>();

        serviceMock.Setup(s => s.GetByIdAsync(99, default)).ReturnsAsync((Todo?)null);

        var controller = new TodosController(serviceMock.Object, loggerMock.Object);
        var result = await controller.GetById(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_WhenDeleted_ReturnsNoContent()
    {
        var serviceMock = new Mock<ITodoService>();
        var loggerMock = new Mock<ILogger<TodosController>>();

        serviceMock.Setup(s => s.DeleteAsync(1, default)).ReturnsAsync(true);

        var controller = new TodosController(serviceMock.Object, loggerMock.Object);
        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }
}
