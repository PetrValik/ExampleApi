using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ExampleApi.Configuration;
using ExampleApi.Features.Auth.GetToken;

namespace ExampleApi.UnitTests.Features.Auth.GetToken;

/// <summary>
/// Unit tests for GetTokenHandler.
/// </summary>
public class GetTokenHandlerTests
{
    /// <summary>
    /// Deterministic JWT settings used across the handler tests.
    /// </summary>
    private static readonly JwtSettings Settings = new()
    {
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        SecretKey = "test-secret-key-that-is-at-least-32-characters-long",
        ExpirationMinutes = 30
    };

    private static GetTokenHandler CreateSut() => new(Options.Create(Settings));

    /// <summary>
    /// Valid credentials return a signed token expiring at the configured lifetime.
    /// </summary>
    [Fact]
    public void Handle_WithValidCredentials_ReturnsTokenWithExpiry()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var response = CreateSut().Handle(new TokenRequest("admin", "admin"));

        // Assert
        response.Should().NotBeNull();
        response!.Token.Should().NotBeNullOrWhiteSpace();
        response.ExpiresAt.Should().BeCloseTo(
            before.AddMinutes(Settings.ExpirationMinutes), TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// The issued token carries the configured issuer, audience and the user name claim.
    /// </summary>
    [Fact]
    public void Handle_WithValidCredentials_TokenCarriesIssuerAudienceAndName()
    {
        // Act
        var response = CreateSut().Handle(new TokenRequest("admin", "admin"));

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response!.Token);
        jwt.Issuer.Should().Be(Settings.Issuer);
        jwt.Audiences.Should().Contain(Settings.Audience);
        jwt.Claims.Should().Contain(claim => claim.Value == "admin");
    }

    /// <summary>
    /// Invalid or empty credentials return null (endpoint maps this to 401). The check is case-sensitive.
    /// </summary>
    [Theory]
    [InlineData("admin", "wrong")]
    [InlineData("wrong", "admin")]
    [InlineData("", "")]
    [InlineData("Admin", "admin")]
    [InlineData("admin", "Admin")]
    public void Handle_WithInvalidCredentials_ReturnsNull(string username, string password)
    {
        // Act
        var response = CreateSut().Handle(new TokenRequest(username, password));

        // Assert
        response.Should().BeNull();
    }
}
