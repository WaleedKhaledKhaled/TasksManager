using System.ComponentModel.DataAnnotations;

namespace TasksManager.Api.DTOs;

public abstract class BaseFilter
{
    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public bool IsDesc { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }
}
