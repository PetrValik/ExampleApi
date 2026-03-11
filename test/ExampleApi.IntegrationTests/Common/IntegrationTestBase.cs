using Microsoft.Extensions.DependencyInjection;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.IntegrationTests.Common;

/// <summary>
/// Base class for integration tests providing common setup and cleanup.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected TestWebApplicationFactory Factory = null!;
    protected HttpClient Client = null!;

    public async Task InitializeAsync()
    {
        Factory = new TestWebApplicationFactory();
        await Factory.InitializeAsync();
        Client = Factory.CreateClient();

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
