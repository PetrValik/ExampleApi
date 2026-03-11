using Microsoft.EntityFrameworkCore;
using ExampleApi.Common.Pagination;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.Features.Articles.Shared.Mappings;
using ExampleApi.Features.Articles.Shared.Models;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.Features.Articles.GetArticles;

/// <summary>
/// Handles retrieval of articles with optional filtering and pagination.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetArticlesHandler"/> class.
/// </remarks>
/// <param name="dbContext">The database context.</param>
public sealed class GetArticlesHandler(AppDbContext dbContext) : IGetArticlesHandler
{
    /// <summary>
    /// Defines the maximum allowed page size for pagination to prevent excessive data retrieval.
    /// </summary>
    private const int MaxPageSize = 100;

    /// <summary>
    /// Gets articles based on the provided filters and pagination parameters.
    /// </summary>
    /// <param name="request">The filter and pagination request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated response of article responses.</returns>
    public async Task<PagedResponse<ArticleResponse>> HandleAsync(
        GetArticlesRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Articles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(article => 
                EF.Functions.Like(article.Name, $"%{request.Name}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(article => article.Category == request.Category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page ?? 1);
        var pageSize = Math.Clamp(request.PageSize ?? 10, 1, MaxPageSize);

        var articles = await query
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
}
