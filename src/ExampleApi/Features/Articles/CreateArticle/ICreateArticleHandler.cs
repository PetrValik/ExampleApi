using ExampleApi.Features.Articles.Shared.DTOs;

namespace ExampleApi.Features.Articles.CreateArticle;

/// <summary>
/// Defines the contract for creating articles.
/// </summary>
public interface ICreateArticleHandler
{
    /// <summary>
    /// Creates a new article.
    /// </summary>
    /// <param name="request">The create article request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created article response.</returns>
    Task<ArticleResponse> HandleAsync(ArticleRequest request, CancellationToken cancellationToken);
}
