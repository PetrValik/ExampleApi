namespace ExampleApi.Configuration;

/// <summary>
/// JWT authentication configuration settings.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>
    /// Configuration section name used to bind JWT settings.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Gets the expected token issuer (<c>iss</c> claim).
    /// </summary>
    public required string Issuer { get; init; }

    /// <summary>
    /// Gets the expected token audience (<c>aud</c> claim).
    /// </summary>
    public required string Audience { get; init; }

    /// <summary>
    /// HMAC-SHA256 signing key — must be at least 32 characters long.
    /// Use a strong random value in production; never commit the real key to source control.
    /// </summary>
    public required string SecretKey { get; init; }

    /// <summary>
    /// Gets the token lifetime in minutes. Default is 60.
    /// </summary>
    public int ExpirationMinutes { get; init; } = 60;
}
