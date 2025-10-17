namespace TasksManager.Api.DTOs.Auth;

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    Guid UserId,
    string Email,
    string Name
);
