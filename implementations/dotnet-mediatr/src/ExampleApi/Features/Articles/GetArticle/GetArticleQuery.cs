using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using MediatR;

namespace ExampleApi.Features.Articles.GetArticle;

/// <summary>
/// Fetches a single article by id (404 when absent).
/// </summary>
/// <param name="Id">The article identifier.</param>
public sealed record GetArticleQuery(int Id) : IRequest<Result<ArticleResponse>>;
