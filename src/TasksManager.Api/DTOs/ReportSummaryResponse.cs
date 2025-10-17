using TasksManager.Api.Models;

namespace TasksManager.Api.DTOs;

public class ReportSummaryResponse
{
    public int TotalTasks { get; set; }
    public IReadOnlyCollection<StatusSummary> Statuses { get; set; } = Array.Empty<StatusSummary>();
    public IReadOnlyCollection<PrioritySummary> Priorities { get; set; } = Array.Empty<PrioritySummary>();
    public int OverdueTasks { get; set; }
    public int CompletingThisWeek { get; set; }
    public int CompletingThisMonth { get; set; }
    public double? AverageCompletionTimeInDays { get; set; }
}

public record StatusSummary(Models.TaskStatus Status, int Count, double Percentage);

public record PrioritySummary(TaskPriority Priority, int Count);
