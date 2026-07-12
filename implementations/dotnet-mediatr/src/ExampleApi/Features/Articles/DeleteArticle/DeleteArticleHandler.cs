using ExampleApi.Common.Results;
using ExampleApi.Infrastructure.Database;
using MediatR;

namespace ExampleApi.Features.Articles.DeleteArticle;

/// <summary>
/// Deletes an article, returning a not-found failure when it does not exist.
/// </summary>
internal sealed class DeleteArticleHandler(AppDbContext dbContext)
    : IRequestHandler<DeleteArticleCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> Handle(DeleteArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await dbContext.Articles.FindAsync([request.Id], cancellationToken);

        if (article is null)
        {
            return Result.Failure(Error.NotFound($"Article with ID {request.Id} was not found."));
        }

        dbContext.Articles.Remove(article);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
