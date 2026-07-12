using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExampleApi.Infrastructure.Persistence;

/// <summary>
/// Creates the database schema at startup, retrying while the PostgreSQL container
/// finishes coming up. Uses <c>EnsureCreated</c> so no migration assets are required.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DbInitializer));
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        const int maxRetries = 10;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await dbContext.Database.EnsureCreatedAsync(cts.Token);
                logger.LogInformation("Database schema is ready.");
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger.LogWarning(
                    ex,
                    "Database initialization attempt {Attempt}/{MaxRetries} failed; retrying in {Delay}s.",
                    attempt,
                    maxRetries,
                    delay.TotalSeconds);
                await Task.Delay(delay);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
            }
        }

        // Final attempt outside the retry guard so a genuine failure surfaces loudly.
        using var finalCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await dbContext.Database.EnsureCreatedAsync(finalCts.Token);
        logger.LogInformation("Database schema is ready.");
    }
}
