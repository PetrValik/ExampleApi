using ExampleApi.Data;
using ExampleApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IArticleRepository"/> over PostgreSQL.
/// </summary>
public sealed class ArticleRepository(AppDbContext dbContext) : IArticleRepository
{
    /// <inheritdoc />
    public async Task<Article?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await dbContext.Articles
            .AsNoTracking()
            .FirstOrDefaultAsync(article => article.ArticleId == id, cancellationToken);

    /// <inheritdoc />
    public async Task<Article?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken) =>
        await dbContext.Articles
            .FirstOrDefaultAsync(article => article.ArticleId == id, cancellationToken);

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Article> Items, int TotalCount)> ListAsync(
        string? name, string? category, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Articles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            // ILike -> PostgreSQL ILIKE for the contract-required case-insensitive partial match.
            var escaped = EscapeLikePattern(name);
            query = query.Where(article => EF.Functions.ILike(article.Name, $"%{escaped}%"));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(article => article.Category == category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(article => article.ArticleId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<Article> AddAsync(Article article, CancellationToken cancellationToken)
    {
        dbContext.Articles.Add(article);
        await dbContext.SaveChangesAsync(cancellationToken);
        return article;
    }

    /// <inheritdoc />
    public async Task<bool> SaveUpdateAsync(Article tracked, uint expectedRowVersion, CancellationToken cancellationToken)
    {
        // Force EF Core to compare the client's row version against the current DB xmin,
        // producing "WHERE xmin = @expected" and a concurrency exception on mismatch.
        dbContext.Entry(tracked).Property(article => article.RowVersion).OriginalValue = expectedRowVersion;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var article = await dbContext.Articles
            .FirstOrDefaultAsync(entity => entity.ArticleId == id, cancellationToken);

        if (article is null)
        {
            return false;
        }

        dbContext.Articles.Remove(article);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Escapes LIKE wildcards in user input. EF Core parameterizes values (no SQL injection) but
    /// does not escape <c>%</c>/<c>_</c>, so we do it manually using the PostgreSQL default backslash.
    /// </summary>
    private static string EscapeLikePattern(string pattern) =>
        pattern
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);
}
