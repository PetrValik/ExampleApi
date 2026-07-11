using ExampleApi.Features.Auth.GetToken;

namespace ExampleApi.Features.Auth;

/// <summary>
/// Provides extension methods for registering the authentication feature services.
/// </summary>
public static class AuthRegistration
{
    /// <summary>
    /// Registers all services required for the auth feature.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IGetTokenHandler, GetTokenHandler>();

        return services;
    }
}
