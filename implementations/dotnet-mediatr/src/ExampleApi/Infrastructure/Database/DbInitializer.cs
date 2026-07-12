using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExampleApi.Infrastructure.Database;

/// <summary>
/// Creates the database schema at startup, retrying while the Postgres container
/// is still coming up (the compose healthcheck plus this retry loop cover readiness).
/// </summary>
public static class DbInitializer
{
    /// <summary>Ensures the schema exists, with exponential-backoff retry on connect failures.</summary>
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
                logger.LogInformation("Database initialization completed successfully.");
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
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to initialize the database after {MaxRetries} attempts.", maxRetries);
                throw;
            }
        }
    }
}
