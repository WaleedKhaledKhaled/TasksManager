using Microsoft.EntityFrameworkCore;
using TasksManager.Api.Data;
using TasksManager.Api.DTOs;
using TasksManager.Api.Models;
using TasksManager.Api.Services.Interfaces;

namespace TasksManager.Api.Services;

/// <summary>
/// Report service
/// </summary>
/// <param name="dbContext"></param>
public class ReportService(AppDbContext dbContext) : IReportService
{
    /// <summary>
    /// Get System Summary
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ReportSummaryResponse> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.UserId == userId);

        var tasks = await query.ToListAsync(cancellationToken);
        var total = tasks.Count;
        var now = DateTime.UtcNow;

        var statusGroups = tasks
            .GroupBy(task => task.Status)
            .Select(group => new StatusSummary(
                group.Key,
                group.Count(),
                total == 0 ? 0 : Math.Round(group.Count() / (double)total * 100, 2)))
            .OrderBy(summary => summary.Status)
            .ToArray();

        var priorityGroups = tasks
            .GroupBy(task => task.Priority)
            .Select(group => new PrioritySummary(group.Key, group.Count()))
            .OrderBy(summary => summary.Priority)
            .ToArray();

        var overdue = tasks.Count(task => task.DueDate.HasValue && task.DueDate.Value < now && task.Status != Models.TaskStatus.Done);

        var weekEnd = now.Date.AddDays(7).AddSeconds(-1);
        var monthEnd = now.Date.AddMonths(1).AddSeconds(-1);

        var completingThisWeek = tasks.Count(task => task.DueDate.HasValue && task.DueDate.Value >= now && task.DueDate.Value <= weekEnd);
        var completingThisMonth = tasks.Count(task => task.DueDate.HasValue && task.DueDate.Value >= now && task.DueDate.Value <= monthEnd);

        var completedDurations = tasks
            .Where(task => task.Status == Models.TaskStatus.Done && task.CompletedAt.HasValue)
            .Select(task => (task.CompletedAt!.Value - task.CreatedAt).TotalDays)
            .ToArray();

        double? averageCompletionTime = completedDurations.Length == 0
            ? null
            : Math.Round(completedDurations.Average(), 2);

        return new ReportSummaryResponse
        {
            TotalTasks = total,
            Statuses = statusGroups,
            Priorities = priorityGroups,
            OverdueTasks = overdue,
            CompletingThisWeek = completingThisWeek,
            CompletingThisMonth = completingThisMonth,
            AverageCompletionTimeInDays = averageCompletionTime
        };
    }
}
