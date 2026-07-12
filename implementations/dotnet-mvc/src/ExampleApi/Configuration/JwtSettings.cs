namespace ExampleApi.Configuration;

/// <summary>
/// Strongly-typed JWT authentication settings bound from the "Jwt" configuration section.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>The token issuer (<c>iss</c> claim).</summary>
    public required string Issuer { get; init; }

    /// <summary>The token audience (<c>aud</c> claim).</summary>
    public required string Audience { get; init; }

    /// <summary>HMAC-SHA256 signing key; must be at least 32 characters.</summary>
    public required string SecretKey { get; init; }

    /// <summary>Token lifetime in minutes (default 60).</summary>
    public int ExpirationMinutes { get; init; } = 60;
}
