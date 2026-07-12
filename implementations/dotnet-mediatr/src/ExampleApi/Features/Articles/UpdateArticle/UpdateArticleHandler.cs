using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using ExampleApi.Features.Articles.Shared.Mappings;
using ExampleApi.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Features.Articles.UpdateArticle;

/// <summary>
/// Updates an article, enforcing optimistic concurrency against the PostgreSQL <c>xmin</c>
/// token: the client's <c>row_version</c> is set as the original value so EF Core includes
/// it in the UPDATE's WHERE clause; a mismatch surfaces as a conflict (409).
/// </summary>
internal sealed class UpdateArticleHandler(AppDbContext dbContext)
    : IRequestHandler<UpdateArticleCommand, Result<ArticleResponse>>
{
    /// <inheritdoc />
    public async Task<Result<ArticleResponse>> Handle(
        UpdateArticleCommand request,
        CancellationToken cancellationToken)
    {
        var article = await dbContext.Articles.FindAsync([request.Id], cancellationToken);

        if (article is null)
        {
            return Result.Failure<ArticleResponse>(
                Error.NotFound($"Article with ID {request.Id} was not found."));
        }

        var payload = request.Request;

        // Compare the client's row version against the current DB value on save.
        dbContext.Entry(article).Property(entity => entity.RowVersion).OriginalValue =
            payload.RowVersion ?? 0u;

        article.Name = payload.Name;
        article.Description = payload.Description;
        article.Category = payload.Category;
        article.Price = payload.Price;
        article.Currency = payload.Currency;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<ArticleResponse>(
                Error.Conflict($"Article with ID {request.Id} was modified by another request. Please retry."));
        }

        return article.ToResponse();
    }
}
