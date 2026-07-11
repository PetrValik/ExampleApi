using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.IntegrationTests.Common;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// Spins up a real PostgreSQL instance via Testcontainers for realistic end-to-end testing.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>
    /// Testcontainers-managed PostgreSQL instance used as the backing store for all integration tests.
    /// </summary>
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("exampleapi_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    /// <summary>
    /// Starts the PostgreSQL container before any test runs.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    /// <summary>
    /// Replaces the production <see cref="AppDbContext"/> registration with one that targets
    /// the Testcontainers PostgreSQL instance, then runs EF Core migrations.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();
        });

        builder.UseEnvironment("Development");
    }

    /// <summary>
    /// Stops and disposes the PostgreSQL container after all tests have finished.
    /// </summary>
    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
