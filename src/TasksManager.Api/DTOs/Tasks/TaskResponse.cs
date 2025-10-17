using TasksManager.Api.Models;

namespace TasksManager.Api.DTOs.Tasks;

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    Models.TaskStatus Status,
    TaskPriority Priority,
    DateTime CreatedAt,
    DateTime? DueDate,
    DateTime? CompletedAt
);
