using ExampleApi.Common.Results;
using MediatR;

namespace ExampleApi.Features.Articles.DeleteArticle;

/// <summary>
/// Deletes an article by id (404 when absent).
/// </summary>
/// <param name="Id">The article identifier.</param>
public sealed record DeleteArticleCommand(int Id) : IRequest<Result>;
