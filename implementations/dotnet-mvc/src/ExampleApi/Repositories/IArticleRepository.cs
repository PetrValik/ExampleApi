using ExampleApi.Models;

namespace ExampleApi.Repositories;

/// <summary>
/// Persistence boundary for <see cref="Article"/>. Encapsulates all EF Core / Npgsql concerns so
/// the service and controller layers stay ignorant of the data provider.
/// </summary>
public interface IArticleRepository
{
    /// <summary>Reads a single article without change tracking. Returns <c>null</c> when absent.</summary>
    Task<Article?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>Reads a single article with change tracking (for a subsequent update).</summary>
    Task<Article?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>Returns a page of articles matching the filters, plus the total matching count.</summary>
    Task<(IReadOnlyList<Article> Items, int TotalCount)> ListAsync(
        string? name, string? category, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Inserts a new article and returns it with its generated id and row version.</summary>
    Task<Article> AddAsync(Article article, CancellationToken cancellationToken);

    /// <summary>
    /// Persists changes to a tracked entity using <paramref name="expectedRowVersion"/> as the
    /// optimistic-concurrency guard. Returns <c>false</c> when the row was modified concurrently.
    /// </summary>
    Task<bool> SaveUpdateAsync(Article tracked, uint expectedRowVersion, CancellationToken cancellationToken);

    /// <summary>Deletes an article by id. Returns <c>false</c> when it does not exist.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
