using ExampleApi.Application.Abstractions;
using ExampleApi.Application.Common.Exceptions;

namespace ExampleApi.Application.Articles.UseCases;

/// <summary>Use case: delete an article by id.</summary>
public interface IDeleteArticleHandler
{
    Task HandleAsync(int articleId, CancellationToken cancellationToken);
}

/// <inheritdoc />
public sealed class DeleteArticleHandler(IArticleRepository repository, IUnitOfWork unitOfWork)
    : IDeleteArticleHandler
{
    public async Task HandleAsync(int articleId, CancellationToken cancellationToken)
    {
        var article = await repository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new NotFoundException($"Article with ID {articleId} was not found.");

        repository.Remove(article);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
