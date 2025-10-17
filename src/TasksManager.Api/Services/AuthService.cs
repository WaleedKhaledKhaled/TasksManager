using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TasksManager.Api.Configuration;
using TasksManager.Api.Data;
using TasksManager.Api.DTOs.Auth;
using TasksManager.Api.Models;
using TasksManager.Api.Services.Interfaces;

namespace TasksManager.Api.Services;

/// <summary>
/// auth service
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="jwtOptions"></param>
    /// <param name="logger"></param>
    public AuthService(AppDbContext dbContext, IOptions<JwtOptions> jwtOptions, ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Register User
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLower();

        if (await _dbContext.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken))
        {
            throw new InvalidOperationException("The provided email address is already registered.");
        }

        // Create a new user record with a fresh identifier and hashed password
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            Name = request.Name.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Email} registered successfully", user.Email);

        return GenerateAuthResponse(user);
    }

    /// <summary>
    /// Login User
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Find the user by email and confirm the password matches the stored hash
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Failed login attempt for {Email}", normalizedEmail);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var passwordMatches = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordMatches)
        {
            _logger.LogWarning("Failed login attempt for {Email}", normalizedEmail);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return GenerateAuthResponse(user);
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        // Build a token that carries the user identity information for the client
        if (string.IsNullOrWhiteSpace(_jwtOptions.Secret))
        {
            throw new InvalidOperationException("JWT secret is not configured.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.TokenLifetimeMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: signingCredentials);

        var handler = new JwtSecurityTokenHandler();
        var tokenValue = handler.WriteToken(token);

        return new AuthResponse(tokenValue, expires, user.Id, user.Email, user.Name);
    }
}
