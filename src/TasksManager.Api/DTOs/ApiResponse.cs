using System;
using System.Collections.Generic;
using System.Linq;

namespace TasksManager.Api.DTOs;

public static class ApiStatus
{
    public const string Success = "success";
    public const string Error = "error";
}

public class ApiResponse<T>
{
    public string Status { get; init; } = ApiStatus.Success;
    public T? Result { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();
}

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T? result) => new()
    {
        Status = ApiStatus.Success,
        Result = result,
        Errors = Array.Empty<string>()
    };

    public static ApiResponse<T> Success<T>() => new()
    {
        Status = ApiStatus.Success,
        Result = default,
        Errors = Array.Empty<string>()
    };

    public static ApiResponse<T> Failure<T>(IEnumerable<string> errors) => new()
    {
        Status = ApiStatus.Error,
        Result = default,
        Errors = errors.ToArray()
    };

    public static ApiResponse<T> Failure<T>(string error) => Failure<T>(new[] { error });
}
