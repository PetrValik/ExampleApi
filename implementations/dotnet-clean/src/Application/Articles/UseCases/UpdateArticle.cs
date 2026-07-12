using ExampleApi.Application.Abstractions;
using ExampleApi.Application.Articles.Dtos;
using ExampleApi.Application.Articles.Mapping;
using ExampleApi.Application.Common.Exceptions;
using ExampleApi.Domain.ValueObjects;

namespace ExampleApi.Application.Articles.UseCases;

/// <summary>Use case: update an existing article with optimistic concurrency.</summary>
public interface IUpdateArticleHandler
{
    Task<ArticleResponse> HandleAsync(int articleId, UpdateArticleRequest request, CancellationToken cancellationToken);
}

/// <inheritdoc />
public sealed class UpdateArticleHandler(IArticleRepository repository, IUnitOfWork unitOfWork)
    : IUpdateArticleHandler
{
    public async Task<ArticleResponse> HandleAsync(
        int articleId,
        UpdateArticleRequest request,
        CancellationToken cancellationToken)
    {
        var article = await repository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new NotFoundException($"Article with ID {articleId} was not found.");

        // Optimistic-concurrency gate: the caller must present the version it last read.
        // A mismatch means the row moved on underneath it — reject with 409.
        // (The validator has already guaranteed request.RowVersion is present and ≥ 1.)
        if (article.RowVersion != request.RowVersion!.Value)
        {
            throw new ConflictException(
                $"Article with ID {articleId} was modified by another request. Please retry with the latest row_version.");
        }

        article.Update(
            request.Name,
            request.Description,
            request.Category,
            new Money(request.Price, request.Currency));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return article.ToResponse();
    }
}
