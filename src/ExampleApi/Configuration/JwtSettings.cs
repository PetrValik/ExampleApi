namespace ExampleApi.Configuration;

/// <summary>
/// JWT authentication configuration settings.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; init; }
    public required string Audience { get; init; }

    /// <summary>
    /// HMAC-SHA256 signing key — must be at least 32 characters long.
    /// Use a strong random value in production; never commit the real key to source control.
    /// </summary>
    public required string SecretKey { get; init; }

    public int ExpirationMinutes { get; init; } = 60;
}
