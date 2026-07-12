using ExampleApi.Application.Abstractions;
using ExampleApi.Application.Articles.Dtos;
using ExampleApi.Application.Articles.Mapping;

namespace ExampleApi.Application.Articles.UseCases;

/// <summary>
/// Query parameters for listing articles. All optional; defaults and clamping are
/// applied by the handler.
/// </summary>
public sealed record GetArticlesQuery(string? Name, string? Category, int? Page, int? PageSize);

/// <summary>Use case: list articles with filtering and pagination.</summary>
public interface IGetArticlesHandler
{
    Task<PagedResponse<ArticleResponse>> HandleAsync(GetArticlesQuery query, CancellationToken cancellationToken);
}

/// <inheritdoc />
public sealed class GetArticlesHandler(IArticleRepository repository) : IGetArticlesHandler
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    public async Task<PagedResponse<ArticleResponse>> HandleAsync(
        GetArticlesQuery query,
        CancellationToken cancellationToken)
    {
        // Defaults + clamp (a too-large pageSize is capped, never rejected).
        var page = Math.Max(1, query.Page ?? 1);
        var pageSize = Math.Clamp(query.PageSize ?? DefaultPageSize, 1, MaxPageSize);

        var result = await repository.ListAsync(query.Name, query.Category, page, pageSize, cancellationToken);
        var items = result.Items.Select(article => article.ToResponse()).ToList();

        return PagedResponse<ArticleResponse>.Create(items, page, pageSize, result.TotalCount);
    }
}
