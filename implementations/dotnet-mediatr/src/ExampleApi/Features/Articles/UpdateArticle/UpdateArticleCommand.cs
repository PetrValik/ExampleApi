using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using MediatR;

namespace ExampleApi.Features.Articles.UpdateArticle;

/// <summary>
/// Replaces an article's fields under optimistic concurrency (stale row_version → 409).
/// </summary>
/// <param name="Id">The article identifier.</param>
/// <param name="Request">The update payload (includes the required row_version).</param>
public sealed record UpdateArticleCommand(int Id, UpdateArticleRequest Request)
    : IRequest<Result<ArticleResponse>>;
