using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using ExampleApi.Features.Articles.Shared.Mappings;
using ExampleApi.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Features.Articles.GetArticle;

/// <summary>
/// Reads a single article, returning a not-found failure when it does not exist.
/// </summary>
internal sealed class GetArticleHandler(AppDbContext dbContext)
    : IRequestHandler<GetArticleQuery, Result<ArticleResponse>>
{
    /// <inheritdoc />
    public async Task<Result<ArticleResponse>> Handle(
        GetArticleQuery request,
        CancellationToken cancellationToken)
    {
        var article = await dbContext.Articles
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.ArticleId == request.Id, cancellationToken);

        return article is null
            ? Result.Failure<ArticleResponse>(Error.NotFound($"Article with ID {request.Id} was not found."))
            : article.ToResponse();
    }
}
