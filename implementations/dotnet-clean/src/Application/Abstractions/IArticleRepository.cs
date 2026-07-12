using ExampleApi.Application.Common;
using ExampleApi.Domain.Entities;

namespace ExampleApi.Application.Abstractions;

/// <summary>
/// Persistence port for the <see cref="Article"/> aggregate. Implemented in the
/// Infrastructure layer (EF Core + PostgreSQL). Staged writes are committed via
/// <see cref="IUnitOfWork"/>.
/// </summary>
public interface IArticleRepository
{
    /// <summary>
    /// Stages a new article for insertion. Its identifier and row version are populated
    /// once <see cref="IUnitOfWork.SaveChangesAsync"/> commits.
    /// </summary>
    Task AddAsync(Article article, CancellationToken cancellationToken);

    /// <summary>
    /// Loads a tracked article by id, or <c>null</c> if it does not exist. The instance is
    /// change-tracked so a subsequent <see cref="IUnitOfWork.SaveChangesAsync"/> persists
    /// mutations applied via the domain methods.
    /// </summary>
    Task<Article?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a filtered, paged slice of articles plus the total match count.
    /// Name is matched partially and case-insensitively; category is matched exactly.
    /// </summary>
    Task<PagedResult<Article>> ListAsync(
        string? name,
        string? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Stages an article for deletion.
    /// </summary>
    void Remove(Article article);
}
