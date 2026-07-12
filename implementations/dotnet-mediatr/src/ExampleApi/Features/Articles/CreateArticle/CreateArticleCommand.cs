using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using MediatR;

namespace ExampleApi.Features.Articles.CreateArticle;

/// <summary>
/// Creates a single article.
/// </summary>
/// <param name="Article">The create request payload.</param>
public sealed record CreateArticleCommand(ArticleRequest Article)
    : IRequest<Result<ArticleResponse>>;
