using Microsoft.EntityFrameworkCore;
using ExampleApi.Common.Exceptions;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.Features.Articles.Shared.Mappings;
using ExampleApi.Features.Articles.Shared.Models;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.Features.Articles.UpdateArticle;

/// <summary>
/// Handles updating an existing article.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UpdateArticleHandler"/> class.
/// </remarks>
/// <param name="dbContext">The database context.</param>
public sealed class UpdateArticleHandler(AppDbContext dbContext) : IUpdateArticleHandler
{

    /// <summary>
    /// Updates an existing article.
    /// </summary>
    /// <param name="articleId">The ID of the article to update.</param>
    /// <param name="request">The update article request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated article response.</returns>
    /// <exception cref="NotFoundException">Thrown when the article is not found.</exception>
    /// <exception cref="ConflictException">Thrown when a concurrency conflict is detected.</exception>
    public async Task<ArticleResponse> HandleAsync(
        int articleId,
        UpdateArticleRequest request,
        CancellationToken cancellationToken)
    {
        var article = await dbContext.Articles.FindAsync([articleId], cancellationToken)
            ?? throw new NotFoundException($"Article with ID {articleId} was not found.");

        // Set the original RowVersion to the value the client had,
        // so EF Core compares it against the current DB value on save.
        dbContext.Entry(article).Property(a => a.RowVersion).OriginalValue = request.RowVersion;

        article.Name = request.Name;
        article.Description = request.Description;
        article.Category = request.Category;
        article.Price = request.Price;
        article.Currency = request.Currency;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException($"Article with ID {articleId} was modified by another request. Please retry.");
        }

        return article.ToResponse();
    }
}
