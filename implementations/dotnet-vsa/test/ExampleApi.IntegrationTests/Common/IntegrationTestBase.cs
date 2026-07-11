using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.IntegrationTests.Common;

/// <summary>
/// Base class for integration tests providing common setup and cleanup.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    /// <summary>
    /// Deserialization model for the token endpoint response.
    /// </summary>
    private sealed record TokenPayload(string Token, DateTime ExpiresAt);

    /// <summary>
    /// The <see cref="TestWebApplicationFactory"/> used for the current test.
    /// </summary>
    protected TestWebApplicationFactory Factory = null!;

    /// <summary>
    /// Pre-authenticated HTTP client for making requests against the test server.
    /// </summary>
    protected HttpClient Client = null!;

    /// <summary>
    /// Starts the factory, authenticates a client and resets the database to a clean state.
    /// </summary>
    public async Task InitializeAsync()
    {
        Factory = new TestWebApplicationFactory();
        await Factory.InitializeAsync();
        Client = Factory.CreateClient();

        // Authenticate with demo credentials and attach the token to all subsequent requests
        var tokenResponse = await Client.PostAsJsonAsync("/auth/token", new { username = "admin", password = "admin" });
        tokenResponse.EnsureSuccessStatusCode();
        var payload = await tokenResponse.Content.ReadFromJsonAsync<TokenPayload>();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload!.Token);

        // Ensure clean database state for each test
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Delete all articles to ensure test isolation
        db.Articles.RemoveRange(db.Articles);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Disposes the HTTP client and the factory (including the Testcontainers PostgreSQL instance).
    /// </summary>
    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (Factory != null)
        {
            await Factory.DisposeAsync();
        }
    }
}
