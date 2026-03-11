namespace ExampleApi.Common.Exceptions;

/// <summary>
/// Exception thrown when a request conflicts with the current state of the resource.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
public sealed class ConflictException(string message) : Exception(message);
