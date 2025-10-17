using TasksManager.Api.DTOs;
using TasksManager.Api.DTOs.Tasks;

namespace TasksManager.Api.Services.Interfaces;

public interface ITaskService
{
    Task<IReadOnlyCollection<TaskResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TaskResponse?> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<TaskResponse> CreateAsync(Guid userId, TaskCreateRequest request, CancellationToken cancellationToken = default);
    Task<TaskResponse> UpdateAsync(Guid userId, Guid taskId, TaskUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<PagedResult<TaskResponse>> FilterAsync(Guid userId, TaskFilterParameters parameters, CancellationToken cancellationToken = default);
}
