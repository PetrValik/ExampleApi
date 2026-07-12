using ExampleApi.Application.Abstractions;
using ExampleApi.Application.Articles.Dtos;
using ExampleApi.Application.Articles.Mapping;
using ExampleApi.Domain.Entities;
using ExampleApi.Domain.ValueObjects;

namespace ExampleApi.Application.Articles.UseCases;

/// <summary>Use case: create a single article.</summary>
public interface ICreateArticleHandler
{
    Task<ArticleResponse> HandleAsync(CreateArticleRequest request, CancellationToken cancellationToken);
}

/// <inheritdoc />
public sealed class CreateArticleHandler(IArticleRepository repository, IUnitOfWork unitOfWork)
    : ICreateArticleHandler
{
    public async Task<ArticleResponse> HandleAsync(CreateArticleRequest request, CancellationToken cancellationToken)
    {
        var article = Article.Create(
            request.Name,
            request.Description,
            request.Category,
            new Money(request.Price, request.Currency));

        await repository.AddAsync(article, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return article.ToResponse();
    }
}
