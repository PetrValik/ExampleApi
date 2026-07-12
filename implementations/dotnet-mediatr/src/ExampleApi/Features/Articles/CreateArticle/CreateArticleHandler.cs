using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using ExampleApi.Features.Articles.Shared.Mappings;
using ExampleApi.Infrastructure.Database;
using MediatR;

namespace ExampleApi.Features.Articles.CreateArticle;

/// <summary>
/// Persists a new article and returns it (with its generated id and row version).
/// </summary>
internal sealed class CreateArticleHandler(AppDbContext dbContext)
    : IRequestHandler<CreateArticleCommand, Result<ArticleResponse>>
{
    /// <inheritdoc />
    public async Task<Result<ArticleResponse>> Handle(
        CreateArticleCommand request,
        CancellationToken cancellationToken)
    {
        var article = request.Article.ToEntity();

        dbContext.Articles.Add(article);
        await dbContext.SaveChangesAsync(cancellationToken);

        return article.ToResponse();
    }
}
