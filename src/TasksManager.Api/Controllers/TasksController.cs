using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TasksManager.Api.DTOs;
using TasksManager.Api.DTOs.Tasks;
using TasksManager.Api.Extensions;
using TasksManager.Api.Services.Interfaces;

namespace TasksManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<CollectionResult<TaskResponse>>>> GetTasks(CancellationToken cancellationToken)
    {
        // Return all tasks for the current user so the client can show the dashboard
        var userId = User.GetUserId();
        var tasks = await taskService.GetAllAsync(userId, cancellationToken);
        var result = new CollectionResult<TaskResponse>
        {
            Items = tasks,
            TotalCount = tasks.Count
        };

        return Ok(ApiResponse.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TaskResponse>>> GetTask(Guid id, CancellationToken cancellationToken)
    {
        // Fetch a single task and respond with not found when it is missing
        var userId = User.GetUserId();
        var task = await taskService.GetByIdAsync(userId, id, cancellationToken);
        if (task is null)
        {
            return NotFound(ApiResponse.Failure<TaskResponse>("Task was not found."));
        }

        return Ok(ApiResponse.Success(task));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskResponse>>> CreateTask([FromBody] TaskCreateRequest request, CancellationToken cancellationToken)
    {
        // Create a task that belongs to the authenticated user
        var userId = User.GetUserId();
        var task = await taskService.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, ApiResponse.Success(task));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TaskResponse>>> UpdateTask(Guid id, [FromBody] TaskUpdateRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        try
        {
            // Update the task details and return the fresh version
            var updatedTask = await taskService.UpdateAsync(userId, id, request, cancellationToken);
            return Ok(ApiResponse.Success(updatedTask));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Failure<TaskResponse>("Task was not found."));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        try
        {
            // Remove the task when the user confirms deletion
            await taskService.DeleteAsync(userId, id, cancellationToken);
            return Ok(ApiResponse.Success(true));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Failure<bool>("Task was not found."));
        }
    }

    [HttpGet("filter")]
    public async Task<ActionResult<ApiResponse<PagedResult<TaskResponse>>>> FilterTasks([FromQuery] TaskFilterParameters parameters, CancellationToken cancellationToken)
    {
        // Apply the requested filters and return a paged list of tasks
        var userId = User.GetUserId();
        var result = await taskService.FilterAsync(userId, parameters, cancellationToken);
        return Ok(ApiResponse.Success(result));
    }
}
