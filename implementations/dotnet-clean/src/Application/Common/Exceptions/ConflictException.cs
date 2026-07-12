namespace ExampleApi.Application.Common.Exceptions;

/// <summary>
/// Thrown by a use case when a write conflicts with the current state of a resource —
/// specifically, an optimistic-concurrency mismatch on update. The presentation layer
/// maps this to HTTP 409 (application/problem+json).
/// </summary>
/// <param name="message">A human-readable explanation.</param>
public sealed class ConflictException(string message) : Exception(message);
