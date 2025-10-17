using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TasksManager.Api.Configuration;
using TasksManager.Api.Data;
using TasksManager.Api.DTOs;
using TasksManager.Api.DTOs.Auth;
using TasksManager.Api.Models;
using TasksManager.Api.Services;
using Xunit;

namespace TasksManager.Api.Tests.Services;

public class AuthServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;


        return new AppDbContext(options);
    }

    private static JwtOptions CreateJwtOptions() => new()
    {
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        Secret = "ThisIsASecretKeyForTestingPurposesOnly123",
        TokenLifetimeMinutes = 60
    };

    [Fact]
    public async Task RegisterAsync_ShouldPersistUser()
    {
        await using var context = CreateDbContext();
        var logger = Mock.Of<ILogger<AuthService>>();
        var service = new AuthService(context, Options.Create(CreateJwtOptions()), logger);

        var response = await service.RegisterAsync(new RegisterRequest
        {
            Email = "user@example.com",
            Password = "Password1!",
            Name = "User"
        });

        Assert.Equal("user@example.com", response.Email);
        Assert.Single(context.Users);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        await using var context = CreateDbContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var logger = Mock.Of<ILogger<AuthService>>();
        var service = new AuthService(context, Options.Create(CreateJwtOptions()), logger);

        var response = await service.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password1!"
        });

        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(user.Id, response.UserId);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrow()
    {
        await using var context = CreateDbContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Name = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var logger = Mock.Of<ILogger<AuthService>>();
        var service = new AuthService(context, Options.Create(CreateJwtOptions()), logger);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "WrongPasswordxyz"
        }));
    }
}
