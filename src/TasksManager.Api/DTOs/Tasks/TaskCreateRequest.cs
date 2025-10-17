using System.ComponentModel.DataAnnotations;
using TasksManager.Api.Attributes;
using TasksManager.Api.Models;

namespace TasksManager.Api.DTOs.Tasks;

public class TaskCreateRequest
{
    //can add error message but this must be already handled by frontend
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    //this code be added manually in code if it is always todo(not mentioned in task)
    public Models.TaskStatus Status { get; set; } = Models.TaskStatus.Todo;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    //this is not requested, but could be added
    [FutureDate(ErrorMessage = "Due date must be in the future.")]
    public DateTime? DueDate { get; set; }
}

