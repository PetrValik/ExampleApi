namespace ExampleApi.Configuration;

/// <summary>
/// Bound JWT configuration (section <c>Jwt</c>).
/// </summary>
public sealed class JwtSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "Jwt";

    /// <summary>The token issuer (<c>iss</c>).</summary>
    public required string Issuer { get; init; }

    /// <summary>The token audience (<c>aud</c>).</summary>
    public required string Audience { get; init; }

    /// <summary>The HMAC-SHA256 signing secret (must be at least 32 characters).</summary>
    public required string SecretKey { get; init; }

    /// <summary>The token lifetime in minutes (default 60).</summary>
    public int ExpirationMinutes { get; init; } = 60;
}
