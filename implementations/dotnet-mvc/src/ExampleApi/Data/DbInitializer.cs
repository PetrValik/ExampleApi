using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Data;

/// <summary>
/// Applies EF Core migrations at startup, retrying while the database container becomes ready.
/// </summary>
public static class DbInitializer
{
    public static async Task InitialiseDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DbInitializer));
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        const int maxRetries = 10;
        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await dbContext.Database.MigrateAsync(cts.Token);
                logger.LogInformation("Database initialization completed successfully.");
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger.LogWarning(ex,
                    "Database initialization attempt {Attempt}/{MaxRetries} failed; retrying in {Delay}s.",
                    attempt, maxRetries, delay.TotalSeconds);
                await Task.Delay(delay);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to initialize database after {MaxRetries} attempts.", maxRetries);
                throw;
            }
        }
    }
}
