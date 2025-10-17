using System.ComponentModel.DataAnnotations;

namespace TasksManager.Api.Models;

public class TaskItem
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Todo;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    [Required]
    public Guid UserId { get; set; }

    public User? User { get; set; }
}
