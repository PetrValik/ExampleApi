using ExampleApi.Application.Abstractions;
using ExampleApi.Application.Articles.Dtos;
using ExampleApi.Application.Articles.Mapping;
using ExampleApi.Domain.Entities;
using ExampleApi.Domain.ValueObjects;

namespace ExampleApi.Application.Articles.UseCases;

/// <summary>Use case: create many articles in one request, preserving input order.</summary>
public interface IBatchCreateArticlesHandler
{
    Task<List<ArticleResponse>> HandleAsync(List<CreateArticleRequest> requests, CancellationToken cancellationToken);
}

/// <inheritdoc />
public sealed class BatchCreateArticlesHandler(IArticleRepository repository, IUnitOfWork unitOfWork)
    : IBatchCreateArticlesHandler
{
    public async Task<List<ArticleResponse>> HandleAsync(
        List<CreateArticleRequest> requests,
        CancellationToken cancellationToken)
    {
        var articles = requests
            .Select(request => Article.Create(
                request.Name,
                request.Description,
                request.Category,
                new Money(request.Price, request.Currency)))
            .ToList();

        foreach (var article in articles)
        {
            await repository.AddAsync(article, cancellationToken);
        }

        // One transactional commit for the whole batch; ids and row versions are then
        // populated on every tracked entity, and order matches the input.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return articles.Select(article => article.ToResponse()).ToList();
    }
}
