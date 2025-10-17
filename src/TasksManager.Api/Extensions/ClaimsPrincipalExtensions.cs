using System.Security.Claims;

namespace TasksManager.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                    principal.FindFirstValue("sub");

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new InvalidOperationException("User identifier claim is missing.");
    }
}
