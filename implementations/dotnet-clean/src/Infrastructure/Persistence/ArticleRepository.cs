using ExampleApi.Application.Abstractions;
using ExampleApi.Application.Common;
using ExampleApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Infrastructure.Persistence;

/// <summary>
/// EF Core adapter for <see cref="IArticleRepository"/>.
/// </summary>
public sealed class ArticleRepository(AppDbContext dbContext) : IArticleRepository
{
    public Task AddAsync(Article article, CancellationToken cancellationToken)
    {
        dbContext.Articles.Add(article);
        return Task.CompletedTask;
    }

    public Task<Article?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Articles.FirstOrDefaultAsync(article => article.ArticleId == id, cancellationToken);

    public async Task<PagedResult<Article>> ListAsync(
        string? name,
        string? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Articles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var pattern = $"%{EscapeLikePattern(name)}%";
            // ILIKE is PostgreSQL's case-insensitive LIKE — satisfies the partial,
            // case-insensitive name filter without lowering both sides.
            query = query.Where(article => EF.Functions.ILike(article.Name, pattern));
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

        return new PagedResult<Article>(items, totalCount);
    }

    public void Remove(Article article) => dbContext.Articles.Remove(article);

    // EF parameterizes values (no SQL injection) but does NOT escape LIKE wildcards,
    // so escape %, _ and the backslash escape char itself. Backslash is PostgreSQL's
    // default LIKE/ILIKE escape character.
    private static string EscapeLikePattern(string pattern) =>
        pattern
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);
}
