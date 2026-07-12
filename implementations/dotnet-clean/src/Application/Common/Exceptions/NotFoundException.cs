namespace ExampleApi.Application.Common.Exceptions;

/// <summary>
/// Thrown by a use case when a requested resource does not exist. The presentation
/// layer maps this to HTTP 404 (application/problem+json).
/// </summary>
/// <param name="message">A human-readable explanation.</param>
public sealed class NotFoundException(string message) : Exception(message);
