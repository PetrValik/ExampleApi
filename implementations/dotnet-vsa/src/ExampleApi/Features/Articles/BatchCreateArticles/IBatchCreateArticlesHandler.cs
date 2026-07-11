using ExampleApi.Features.Articles.Shared.DTOs;

namespace ExampleApi.Features.Articles.BatchCreateArticles;

/// <summary>
/// Defines the contract for batch creating articles.
/// </summary>
public interface IBatchCreateArticlesHandler
{
    /// <summary>
    /// Creates multiple articles concurrently.
    /// </summary>
    /// <param name="requests">The list of article requests.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of created article responses.</returns>
    Task<List<ArticleResponse>> HandleAsync(List<ArticleRequest> requests, CancellationToken cancellationToken);
}
