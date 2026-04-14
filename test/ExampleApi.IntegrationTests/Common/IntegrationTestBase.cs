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
    private sealed record TokenPayload(string Token, DateTime ExpiresAt);
    protected TestWebApplicationFactory Factory = null!;
    protected HttpClient Client = null!;

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

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (Factory != null)
        {
            await Factory.DisposeAsync();
        }
    }
}
