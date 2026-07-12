using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using MediatR;

namespace ExampleApi.Features.Articles.BatchCreateArticles;

/// <summary>
/// Creates a batch of articles (1..100). Any invalid item (or an empty batch) → 400.
/// </summary>
/// <param name="Items">The articles to create, in order.</param>
public sealed record BatchCreateArticlesCommand(IReadOnlyList<ArticleRequest> Items)
    : IRequest<Result<IReadOnlyList<ArticleResponse>>>;
