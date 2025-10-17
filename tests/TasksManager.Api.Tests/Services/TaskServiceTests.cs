using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TasksManager.Api.Data;
using TasksManager.Api.DTOs;
using TasksManager.Api.DTOs.Tasks;
using TasksManager.Api.Models;
using TasksManager.Api.Services;
using Xunit;

namespace TasksManager.Api.Tests.Services;

public class TaskServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TaskMappingProfile>();
        });
        configuration.AssertConfigurationIsValid();
        return configuration.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistTask()
    {
        await using var context = CreateDbContext();
        var mapper = CreateMapper();
        var service = new TaskService(context, mapper);
        var userId = Guid.NewGuid();

        var request = new TaskCreateRequest
        {
            Title = "Test Task",
            Description = "Description",
            Priority = TaskPriority.High,
            Status = Models.TaskStatus.Todo
        };

        var response = await service.CreateAsync(userId, request);

        Assert.Equal(request.Title, response.Title);
        Assert.Equal(Models.TaskStatus.Todo, response.Status);
        Assert.Equal(1, context.Tasks.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTask_WhenExists()
    {
        await using var context = CreateDbContext();
        var mapper = CreateMapper();

        var userId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Existing",
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var service = new TaskService(context, mapper);
        var result = await service.GetByIdAsync(userId, task.Id);

        Assert.NotNull(result);
        Assert.Equal(task.Id, result!.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldChangeValues()
    {
        await using var context = CreateDbContext();
        var mapper = CreateMapper();
        var userId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Original",
            Status = Models.TaskStatus.Todo,
            Priority = TaskPriority.Low,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var service = new TaskService(context, mapper);
        var request = new TaskUpdateRequest
        {
            Title = "Updated",
            Description = "New description",
            Status = Models.TaskStatus.Done,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(2)
        };

        var response = await service.UpdateAsync(userId, task.Id, request);

        Assert.Equal("Updated", response.Title);
        Assert.Equal(Models.TaskStatus.Done, response.Status);
        Assert.NotNull(response.CompletedAt);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTask()
    {
        await using var context = CreateDbContext();
        var mapper = CreateMapper();
        var userId = Guid.NewGuid();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "To Delete",
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var service = new TaskService(context, mapper);
        await service.DeleteAsync(userId, task.Id);

        Assert.Empty(context.Tasks);
    }

    [Fact]
    public async Task FilterAsync_ShouldApplyStatusAndPriority()
    {
        await using var context = CreateDbContext();
        var mapper = CreateMapper();
        var userId = Guid.NewGuid();
        context.Tasks.AddRange(
            new TaskItem { Id = Guid.NewGuid(), Title = "Todo Low", Status = Models.TaskStatus.Todo, Priority = TaskPriority.Low, UserId = userId, CreatedAt = DateTime.UtcNow },
            new TaskItem { Id = Guid.NewGuid(), Title = "InProgress High", Status = Models.TaskStatus.InProgress, Priority = TaskPriority.High, UserId = userId, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new TaskService(context, mapper);
        var parameters = new TaskFilterParameters
        {
            Statuses = new() { Models.TaskStatus.InProgress },
            Priorities = new() { TaskPriority.High }
        };

        var result = await service.FilterAsync(userId, parameters);

        Assert.Single(result.Items);
        Assert.Equal("InProgress High", result.Items.First().Title);
    }
}
