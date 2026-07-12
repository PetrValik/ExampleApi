namespace ExampleApi.Application.Abstractions;

/// <summary>
/// Port for issuing signed bearer tokens. Implemented in Infrastructure (HS256 JWT).
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Issues a signed token for the given subject.
    /// </summary>
    /// <param name="username">The authenticated subject placed in the name claim.</param>
    /// <returns>The serialized token and its UTC expiry instant.</returns>
    (string Token, DateTime ExpiresAt) GenerateToken(string username);
}
