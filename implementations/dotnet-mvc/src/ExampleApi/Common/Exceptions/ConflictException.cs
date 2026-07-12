namespace ExampleApi.Common.Exceptions;

/// <summary>Thrown when a write conflicts with the current resource state (optimistic concurrency). Mapped to HTTP 409.</summary>
public sealed class ConflictException(string message) : Exception(message);
