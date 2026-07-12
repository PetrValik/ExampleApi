using ExampleApi.Application.Common.Exceptions;

namespace ExampleApi.Application.Abstractions;

/// <summary>
/// Transactional boundary port. Commits all staged repository changes as one unit.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all staged changes.
    /// </summary>
    /// <exception cref="ConflictException">
    /// Thrown when the underlying store detects a concurrent modification.
    /// </exception>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
