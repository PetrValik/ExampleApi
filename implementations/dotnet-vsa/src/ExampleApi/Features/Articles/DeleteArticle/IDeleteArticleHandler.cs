namespace ExampleApi.Features.Articles.DeleteArticle;

/// <summary>
/// Defines the contract for deleting articles.
/// </summary>
public interface IDeleteArticleHandler
{
    /// <summary>
    /// Deletes an article by its identifier.
    /// </summary>
    /// <param name="id">The article identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task HandleAsync(int id, CancellationToken cancellationToken);
}