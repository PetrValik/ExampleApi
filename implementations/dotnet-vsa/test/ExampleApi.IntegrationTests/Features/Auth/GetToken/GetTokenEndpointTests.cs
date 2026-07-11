using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExampleApi.IntegrationTests.Common;

namespace ExampleApi.IntegrationTests.Features.Auth.GetToken;

/// <summary>
/// Integration tests for the token endpoint (POST /auth/token).
/// </summary>
public class GetTokenEndpointTests : IntegrationTestBase
{
    /// <summary>
    /// Deserialization model for the token endpoint response.
    /// </summary>
    private sealed record TokenPayload(string Token, DateTime ExpiresAt);

    /// <summary>
    /// Valid demo credentials return 200 with a non-empty token that expires in the future.
    /// </summary>
    [Fact]
    public async Task GetToken_WithValidCredentials_Returns200AndToken()
    {
        // Arrange — the endpoint is anonymous, so use a clean unauthenticated client
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/auth/token", new { username = "admin", password = "admin" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<TokenPayload>();
        payload.Should().NotBeNull();
        payload!.Token.Should().NotBeNullOrWhiteSpace();
        payload.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    /// <summary>
    /// Wrong credentials return 401 Unauthorized.
    /// </summary>
    [Theory]
    [InlineData("admin", "wrong")]
    [InlineData("nobody", "admin")]
    [InlineData("", "")]
    public async Task GetToken_WithInvalidCredentials_Returns401(string username, string password)
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/auth/token", new { username, password });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
