using ExampleApi.Features.Articles.Shared.DTOs;

namespace ExampleApi.Features.Articles.GetArticle;

/// <summary>
/// Defines the contract for retrieving a single article.
/// </summary>
public interface IGetArticleHandler
{
    /// <summary>
    /// Gets an article by its ID.
    /// </summary>
    /// <param name="articleId">The article ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The article response.</returns>
    Task<ArticleResponse> HandleAsync(int articleId, CancellationToken cancellationToken);
}
