using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.IntegrationTests.Common;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// Uses SQLite in-memory database for realistic testing with proper SQL support.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _connection;

    public async Task InitializeAsync()
    {
        // Create and open a SQLite in-memory connection
        // Must stay open for the lifetime of the test to keep the database alive
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using the in-memory SQLite connection
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection!);
            });

            // Build service provider to initialize the database
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Apply migrations to create the schema
            db.Database.Migrate();
        });

        builder.UseEnvironment("Development");
    }

    public new async Task DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}
