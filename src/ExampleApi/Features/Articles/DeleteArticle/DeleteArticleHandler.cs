using ExampleApi.Common.Exceptions;
using ExampleApi.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Features.Articles.DeleteArticle;

/// <summary>
/// Handles the deletion of articles.
/// </summary>
/// <param name="dbContext">The database context.</param>
public sealed class DeleteArticleHandler(AppDbContext dbContext) : IDeleteArticleHandler
{
    /// <summary>
    /// Deletes an article by its identifier.
    /// </summary>
    /// <param name="id">The article identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="NotFoundException">Thrown when the article is not found.</exception>
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        var article = await dbContext.Articles
            .FirstOrDefaultAsync(a => a.ArticleId == id, cancellationToken)
            ?? throw new NotFoundException($"Article with ID {id} was not found.");

        dbContext.Articles.Remove(article);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}