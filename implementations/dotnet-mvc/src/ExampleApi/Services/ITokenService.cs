using ExampleApi.Dtos;

namespace ExampleApi.Services;

/// <summary>
/// Issues JWT bearer tokens for the demo identity.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Validates the supplied credentials and, if valid, returns a signed token and its expiry.
    /// Returns <c>null</c> when the credentials are invalid.
    /// </summary>
    TokenResponse? Authenticate(TokenRequest request);
}
