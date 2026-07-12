using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using ExampleApi.Features.Articles.Shared.Mappings;
using ExampleApi.Infrastructure.Database;
using MediatR;

namespace ExampleApi.Features.Articles.BatchCreateArticles;

/// <summary>
/// Persists a batch of articles in a single transaction and returns them (in order) with
/// their generated ids and row versions.
/// </summary>
internal sealed class BatchCreateArticlesHandler(AppDbContext dbContext)
    : IRequestHandler<BatchCreateArticlesCommand, Result<IReadOnlyList<ArticleResponse>>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ArticleResponse>>> Handle(
        BatchCreateArticlesCommand request,
        CancellationToken cancellationToken)
    {
        var articles = request.Items.Select(item => item.ToEntity()).ToList();

        dbContext.Articles.AddRange(articles);
        await dbContext.SaveChangesAsync(cancellationToken);

        IReadOnlyList<ArticleResponse> responses =
            [.. articles.Select(article => article.ToResponse())];

        return Result.Success(responses);
    }
}
