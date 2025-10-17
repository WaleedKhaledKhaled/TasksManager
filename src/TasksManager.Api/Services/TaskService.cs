using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TasksManager.Api.Data;
using TasksManager.Api.DTOs;
using TasksManager.Api.DTOs.Tasks;
using TasksManager.Api.Models;
using TasksManager.Api.Services.Interfaces;

namespace TasksManager.Api.Services;

/// <summary>
/// Task Service
/// </summary>
/// <param name="dbContext"></param>
/// <param name="mapper"></param>
public class TaskService(
    AppDbContext dbContext,//we should've used repository patteren but it is not used
    IMapper mapper
    ) : ITaskService
{
    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Get All Tasks For Specfic User
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IReadOnlyCollection<TaskResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {

        var tasks = await dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.UserId == userId)
            .OrderByDescending(task => task.CreatedAt)
            .ToListAsync(cancellationToken);

        return tasks.Select(MapToResponse).ToArray();
    }

    /// <summary>
    /// Get Task for user by id
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="taskId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TaskResponse?> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.UserId == userId)
            .Where(task => task.Id == taskId)
            .FirstOrDefaultAsync(cancellationToken);

        return task is null ? null : MapToResponse(task);
    }

    /// <summary>
    /// Create Task
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TaskResponse> CreateAsync(Guid userId, TaskCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,//this may be forced to be todo
            Priority = request.Priority,
            CreatedAt = DateTime.UtcNow,
            DueDate = request.DueDate,
            UserId = userId,
            CompletedAt = request.Status == Models.TaskStatus.Done ? DateTime.UtcNow : null
        };

        dbContext.Tasks.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(entity);
    }

    /// <summary>
    /// update task
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="taskId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<TaskResponse> UpdateAsync(Guid userId, Guid taskId, TaskUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Tasks
         .Where(task => task.UserId == userId)
         .Where(task => task.Id == taskId)
         .FirstOrDefaultAsync(cancellationToken);

        if (entity is null) throw new KeyNotFoundException($"Task '{taskId}' was not found.");

        if (entity.Status == Models.TaskStatus.Done) throw new Exception("Done task can't be edited");

        var statusChangedToDone = request.Status == Models.TaskStatus.Done && entity.Status != Models.TaskStatus.Done;

        mapper.Map(request, entity);

        entity.CompletedAt = statusChangedToDone
            ? DateTime.UtcNow
            : request.Status != Models.TaskStatus.Done
                ? null
                : entity.CompletedAt;

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(entity);
    }

    /// <summary>
    /// Delete task
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="taskId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Tasks
        .Where(task => task.UserId == userId)
        .Where(task => task.Id == taskId)
        .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            throw new KeyNotFoundException($"Task '{taskId}' was not found.");
        }
        // mark task as soft deleted instead of removing it from the database
        entity.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Filter Tasks
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<PagedResult<TaskResponse>> FilterAsync(Guid userId, TaskFilterParameters parameters, CancellationToken cancellationToken = default)
    {
        //this filter could be handled in list api(but it seems to be separated in requirements)
        var query = dbContext.Tasks.AsNoTracking().Where(task => task.UserId == userId);

        query = ApplyFilter(parameters, query);

        query = ApplySorting(query, parameters.SortBy, parameters.IsDesc);

        var totalCount = await query.CountAsync(cancellationToken);
        var page = Math.Max(1, parameters.Page);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskResponse>
        {
            Items = items.Select(MapToResponse).ToArray(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    private static IQueryable<TaskItem> ApplyFilter(TaskFilterParameters parameters, IQueryable<TaskItem> query)
    {
        if (parameters.Statuses.Any())
        {
            query = query.Where(task => parameters.Statuses.Contains(task.Status));
        }

        if (parameters.Priorities.Any())
        {
            query = query.Where(task => parameters.Priorities.Contains(task.Priority));
        }

        if (parameters.CreatedFrom.HasValue)
        {
            query = query.Where(task => task.CreatedAt >= parameters.CreatedFrom.Value);
        }

        if (parameters.CreatedTo.HasValue)
        {
            query = query.Where(task => task.CreatedAt <= parameters.CreatedTo.Value);
        }

        if (parameters.DueDateFrom.HasValue)
        {
            query = query.Where(task => task.DueDate >= parameters.DueDateFrom.Value);
        }

        if (parameters.DueDateTo.HasValue)
        {
            query = query.Where(task => task.DueDate <= parameters.DueDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var term = parameters.Search.Trim();
            query = query.Where(task => task.Title.Contains(term) || (task.Description != null && task.Description.Contains(term)));
        }

        return query;
    }

    private static IQueryable<TaskItem> ApplySorting(IQueryable<TaskItem> query, string? sortBy, bool isDescending)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "duedate" => isDescending ? query.OrderByDescending(task => task.DueDate) : query.OrderBy(task => task.DueDate),
            "priority" => isDescending ? query.OrderByDescending(task => task.Priority) : query.OrderBy(task => task.Priority),
            "status" => isDescending ? query.OrderByDescending(task => task.Status) : query.OrderBy(task => task.Status),
            "title" => isDescending ? query.OrderByDescending(task => task.Title) : query.OrderBy(task => task.Title),
            _ => isDescending ? query.OrderByDescending(task => task.CreatedAt) : query.OrderBy(task => task.CreatedAt)
        };
    }

    private static TaskResponse MapToResponse(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.Status,
        task.Priority,
        task.CreatedAt,
        task.DueDate,
        task.CompletedAt);
}
