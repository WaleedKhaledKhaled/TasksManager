using System.ComponentModel.DataAnnotations;
using TasksManager.Api.Models;

namespace TasksManager.Api.DTOs.Tasks;

public class TaskUpdateRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Models.TaskStatus Status { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime? DueDate { get; set; }
}
