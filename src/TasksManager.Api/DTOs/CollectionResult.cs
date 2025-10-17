using System;
using System.Collections.Generic;

namespace TasksManager.Api.DTOs;

public class CollectionResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
}
