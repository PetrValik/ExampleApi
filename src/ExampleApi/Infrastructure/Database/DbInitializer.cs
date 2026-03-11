using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Infrastructure.Database;

/// <summary>
/// Provides database initialization functionality.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database by applying pending migrations.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task InitialiseDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DbInitializer));
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Apply any pending migrations (creates database if it doesn't exist)
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to initialize database");
            throw;
        }
    }
}