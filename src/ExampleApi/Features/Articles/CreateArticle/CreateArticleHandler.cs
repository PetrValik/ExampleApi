using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.Features.Articles.Shared.Mappings;
using ExampleApi.Features.Articles.Shared.Models;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.Features.Articles.CreateArticle;

/// <summary>
/// Handles the creation of new articles.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateArticleHandler"/> class.
/// </remarks>
/// <param name="dbContext">The database context.</param>
public sealed class CreateArticleHandler(AppDbContext dbContext) : ICreateArticleHandler
{

    /// <summary>
    /// Creates a new article.
    /// </summary>
    /// <param name="request">The create article request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created article response.</returns>
    public async Task<ArticleResponse> HandleAsync(
        ArticleRequest request,
        CancellationToken cancellationToken)
    {
        var article = new Article
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Price = request.Price,
            Currency = request.Currency
        };

        dbContext.Articles.Add(article);
        await dbContext.SaveChangesAsync(cancellationToken);

        return article.ToResponse();
    }
}
