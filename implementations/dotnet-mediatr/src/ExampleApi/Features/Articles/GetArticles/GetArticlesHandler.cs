using ExampleApi.Common.Pagination;
using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using ExampleApi.Features.Articles.Shared.Mappings;
using ExampleApi.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Features.Articles.GetArticles;

/// <summary>
/// Applies filters and pagination and returns a page of articles.
/// </summary>
internal sealed class GetArticlesHandler(AppDbContext dbContext)
    : IRequestHandler<GetArticlesQuery, Result<PagedResponse<ArticleResponse>>>
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    /// <inheritdoc />
    public async Task<Result<PagedResponse<ArticleResponse>>> Handle(
        GetArticlesQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Articles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var escaped = EscapeLikePattern(request.Name);
            query = query.Where(article => EF.Functions.Like(article.Name, $"%{escaped}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(article => article.Category == request.Category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(DefaultPage, request.Page ?? DefaultPage);
        var pageSize = Math.Clamp(request.PageSize ?? DefaultPageSize, 1, MaxPageSize);

        var articles = await query
            .OrderBy(article => article.ArticleId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<ArticleResponse>
        {
            Items = [.. articles.Select(article => article.ToResponse())],
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Escapes LIKE wildcards in user input. EF Core parameterizes the query (no SQL
    /// injection) but does not escape <c>%</c> / <c>_</c>, so we do it here (backslash
    /// is the Postgres default escape character).
    /// </summary>
    private static string EscapeLikePattern(string pattern) =>
        pattern
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%", StringComparison.Ordinal)
            .Replace("_", @"\_", StringComparison.Ordinal);
}
