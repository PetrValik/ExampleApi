using ExampleApi.Application.Abstractions;
using ExampleApi.Application.Articles.Dtos;
using ExampleApi.Application.Articles.Mapping;
using ExampleApi.Application.Common.Exceptions;

namespace ExampleApi.Application.Articles.UseCases;

/// <summary>Use case: fetch a single article by id.</summary>
public interface IGetArticleHandler
{
    Task<ArticleResponse> HandleAsync(int articleId, CancellationToken cancellationToken);
}

/// <inheritdoc />
public sealed class GetArticleHandler(IArticleRepository repository) : IGetArticleHandler
{
    public async Task<ArticleResponse> HandleAsync(int articleId, CancellationToken cancellationToken)
    {
        var article = await repository.GetByIdAsync(articleId, cancellationToken)
            ?? throw new NotFoundException($"Article with ID {articleId} was not found.");

        return article.ToResponse();
    }
}
