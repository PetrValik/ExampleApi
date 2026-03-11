using ExampleApi.Features.Articles.Shared.DTOs;

namespace ExampleApi.Features.Articles.UpdateArticle;

/// <summary>
/// Defines the contract for updating articles.
/// </summary>
public interface IUpdateArticleHandler
{
    /// <summary>
    /// Updates an existing article.
    /// </summary>
    /// <param name="articleId">The ID of the article to update.</param>
    /// <param name="request">The update article request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated article response.</returns>
    Task<ArticleResponse> HandleAsync(int articleId, UpdateArticleRequest request, CancellationToken cancellationToken);
}
