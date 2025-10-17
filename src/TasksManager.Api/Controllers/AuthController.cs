using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TasksManager.Api.DTOs;
using TasksManager.Api.DTOs.Auth;
using TasksManager.Api.Services.Interfaces;

namespace TasksManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Create a new account and hand back a fresh token for immediate use
            var response = await authService.RegisterAsync(request, cancellationToken);
            return Ok(ApiResponse.Success(response));
        }
        catch (InvalidOperationException ex)
        {
            var errors = new[] { ex.Message };
            return BadRequest(ApiResponse.Failure<AuthResponse>(errors));
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify credentials and return a token that authorizes future calls
            var response = await authService.LoginAsync(request, cancellationToken);
            return Ok(ApiResponse.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse.Failure<AuthResponse>("Invalid email or password."));
        }
    }
}
