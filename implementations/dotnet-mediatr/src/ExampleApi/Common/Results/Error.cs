namespace ExampleApi.Common.Results;

/// <summary>
/// Classifies a domain error so the HTTP layer can pick the right status code.
/// </summary>
public enum ErrorType
{
    /// <summary>A generic failure (mapped to 400/401 depending on the endpoint).</summary>
    Failure,

    /// <summary>One or more request fields failed validation (mapped to 400).</summary>
    Validation,

    /// <summary>The requested resource does not exist (mapped to 404).</summary>
    NotFound,

    /// <summary>An optimistic-concurrency conflict occurred (mapped to 409).</summary>
    Conflict
}

/// <summary>
/// A typed, non-throwing representation of a business failure carried inside a
/// <see cref="Result"/>.
/// </summary>
/// <param name="Code">A stable machine-readable code.</param>
/// <param name="Description">A human-readable description used as the problem detail.</param>
/// <param name="Type">The category used to map the error to an HTTP status.</param>
public record Error(string Code, string Description, ErrorType Type)
{
    /// <summary>The sentinel "no error" value carried by a successful result.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>Creates a not-found error (HTTP 404).</summary>
    public static Error NotFound(string description) =>
        new("General.NotFound", description, ErrorType.NotFound);

    /// <summary>Creates a conflict error (HTTP 409).</summary>
    public static Error Conflict(string description) =>
        new("General.Conflict", description, ErrorType.Conflict);

    /// <summary>Creates a generic failure error.</summary>
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);
}

/// <summary>
/// A validation error that additionally carries a per-field error map, rendered
/// as the <c>errors</c> object of an RFC 7807 validation problem response.
/// </summary>
/// <param name="Errors">Field name → messages.</param>
public sealed record ValidationError(IDictionary<string, string[]> Errors)
    : Error("General.Validation", "One or more validation errors occurred.", ErrorType.Validation);
