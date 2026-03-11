namespace ExampleApi.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
public sealed class NotFoundException(string message) : Exception(message);
