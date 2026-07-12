namespace ExampleApi.Infrastructure.Authentication;

/// <summary>
/// Strongly-typed JWT configuration, bound from the <c>Jwt</c> configuration section.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>The token issuer (<c>iss</c> claim).</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>The token audience (<c>aud</c> claim).</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>The HMAC-SHA256 signing key. Must be at least 32 characters.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Token lifetime in minutes (default 60).</summary>
    public int ExpirationMinutes { get; set; } = 60;
}
