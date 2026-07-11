using Microsoft.EntityFrameworkCore;
using ExampleApi.Common.Exceptions;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.Features.Articles.Shared.Mappings;
using ExampleApi.Features.Articles.Shared.Models;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.Features.Articles.GetArticle;

/// <summary>
/// Handles retrieval of a single article by ID.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetArticleHandler"/> class.
/// </remarks>
/// <param name="dbContext">The database context.</param>
public sealed class GetArticleHandler(AppDbContext dbContext) : IGetArticleHandler
{

    /// <summary>
    /// Gets an article by its ID.
    /// </summary>
    /// <param name="articleId">The article ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The article response.</returns>
    /// <exception cref="NotFoundException">Thrown when the article is not found.</exception>
    public async Task<ArticleResponse> HandleAsync(
        int articleId,
        CancellationToken cancellationToken)
    {
        var article = await dbContext.Articles
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ArticleId == articleId, cancellationToken);

        return article is null ? throw new NotFoundException($"Article with ID {articleId} not found.") : article.ToResponse();
    }
}
