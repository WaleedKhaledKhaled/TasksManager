using TasksManager.Api.Models;

namespace TasksManager.Api.DTOs.Tasks;

public class TaskFilterParameters : BaseFilter
{
    public List<Models.TaskStatus> Statuses { get; init; } = new();

    public List<TaskPriority> Priorities { get; init; } = new();

    public DateTime? DueDateFrom { get; set; }

    public DateTime? DueDateTo { get; set; }
}
