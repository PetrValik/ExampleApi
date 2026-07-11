using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.Features.Articles.Shared.Mappings;
using ExampleApi.Features.Articles.Shared.Models;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.Features.Articles.BatchCreateArticles;

/// <summary>
/// Handles the concurrent creation of multiple articles.
/// Each article is saved in parallel using a separate DbContext scope.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BatchCreateArticlesHandler"/> class.
/// </remarks>
/// <param name="scopeFactory">The service scope factory for creating independent DbContext instances.</param>
public sealed class BatchCreateArticlesHandler(IServiceScopeFactory scopeFactory) : IBatchCreateArticlesHandler
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    /// <summary>
    /// Creates multiple articles concurrently.
    /// </summary>
    /// <param name="requests">The list of article requests.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of created article responses.</returns>
    public async Task<List<ArticleResponse>> HandleAsync(
        List<ArticleRequest> requests,
        CancellationToken cancellationToken)
    {
        var tasks = requests.Select(async request =>
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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
        });

        var results = await Task.WhenAll(tasks);
        return [.. results];
    }
}
