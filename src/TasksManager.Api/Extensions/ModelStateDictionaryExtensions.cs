using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TasksManager.Api.Extensions;

public static class ModelStateDictionaryExtensions
{
    public static IReadOnlyCollection<string> ToErrors(this ModelStateDictionary modelState)
    {
        return modelState
            .Where(entry => entry.Value != null)
            .SelectMany(entry => entry.Value!.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Invalid value" : error.ErrorMessage)
            .Distinct()
            .ToArray();
    }
}
