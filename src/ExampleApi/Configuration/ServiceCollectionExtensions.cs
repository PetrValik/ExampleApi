using System.Text.Json.Nodes;
using FluentValidation;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using ExampleApi.Common.Endpoints;
using ExampleApi.Features.Articles;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.Configuration;

/// <summary>
/// Extension methods for configuring application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds API documentation services (OpenAPI and Scalar).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer((schema, context, cancellationToken) =>
            {
                if (context.JsonTypeInfo.Type == typeof(ArticleRequest))
                {
                    schema.Example = new JsonObject
                    {
                        ["name"] = "Wireless Keyboard",
                        ["description"] = "A compact wireless keyboard with Bluetooth connectivity.",
                        ["category"] = "Electronics",
                        ["price"] = 49.99,
                        ["currency"] = "USD"
                    };
                }

                return Task.CompletedTask;
            });
        });

        return services;
    }

    /// <summary>
    /// Configures routing options for SlimBuilder (adds regex constraint support).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRoutingConfiguration(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options =>
            options.SetParameterPolicy<RegexInlineRouteConstraint>("regex"));
        
        return services;
    }

    /// <summary>
    /// Adds database context with SQLite provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when connection string 'Default' is not found.</exception>
    public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlite(configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' not found in configuration.")));

        return services;
    }

    /// <summary>
    /// Adds application features and validators.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddArticlesFeature();
        services.AddEndpoints();
        
        return services;
    }
}
