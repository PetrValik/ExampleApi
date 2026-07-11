namespace ExampleApi.Features.Auth.GetToken;

/// <summary>
/// Defines the contract for issuing JWT tokens.
/// </summary>
public interface IGetTokenHandler
{
    /// <summary>
    /// Validates the supplied credentials and issues a signed JWT when they are valid.
    /// </summary>
    /// <param name="request">The token request carrying the credentials.</param>
    /// <returns>
    /// A <see cref="GetTokenResponse"/> when the credentials are valid; otherwise <see langword="null"/>.
    /// </returns>
    GetTokenResponse? Handle(GetTokenRequest request);
}
