using ExampleApi.Common.Pagination;
using ExampleApi.Features.Articles.Shared.DTOs;

namespace ExampleApi.Features.Articles.GetArticles;

/// <summary>
/// Defines the contract for retrieving articles with optional filtering and pagination.
/// </summary>
public interface IGetArticlesHandler
{
    /// <summary>
    /// Gets articles based on the provided filters and pagination parameters.
    /// </summary>
    /// <param name="request">The filter and pagination request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated response of article responses.</returns>
    Task<PagedResponse<ArticleResponse>> HandleAsync(GetArticlesRequest request, CancellationToken cancellationToken);
}
