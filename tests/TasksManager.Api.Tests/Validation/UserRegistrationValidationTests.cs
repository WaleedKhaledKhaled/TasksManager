using System.ComponentModel.DataAnnotations;
using TasksManager.Api.DTOs.Auth;
using Xunit;

namespace TasksManager.Api.Tests.Validation;

public class UserRegistrationValidationTests
{
    [Fact]
    public void RegisterRequest_ShouldBeInvalid_WhenEmailMissing()
    {
        var request = new RegisterRequest
        {
            Email = "",
            Password = "Password1!",
            Name = "Sample User"
        };

        var results = Validate(request);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterRequest.Email)));
    }

    [Fact]
    public void RegisterRequest_ShouldBeInvalid_WhenPasswordTooShort()
    {
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "123",
            Name = "Sample User"
        };

        var results = Validate(request);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterRequest.Password)));
    }

    [Fact]
    public void RegisterRequest_ShouldBeInvalid_WhenNameMissing()
    {
        var request = new RegisterRequest
        {
            Email = "user@example.com",
            Password = "Password1!",
            Name = string.Empty
        };

        var results = Validate(request);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterRequest.Name)));
    }

    private static IList<ValidationResult> Validate(RegisterRequest request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, context, results, true);
        return results;
    }
}
