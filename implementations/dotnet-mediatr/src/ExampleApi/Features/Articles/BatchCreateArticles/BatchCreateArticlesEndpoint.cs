using System.Collections.Generic;
using ExampleApi.Common.Endpoints;
using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ExampleApi.Features.Articles.BatchCreateArticles;

/// <summary>
/// <c>POST /api/articles-concurrent</c> — batch create. 201 with the created array,
/// 400 on an empty batch or any invalid item. Requires a bearer token.
/// </summary>
internal sealed class BatchCreateArticlesEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/articles-concurrent", async (
                List<ArticleRequest> request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new BatchCreateArticlesCommand(request), cancellationToken);

                return result.IsSuccess
                    ? Results.Json(result.Value, statusCode: StatusCodes.Status201Created)
                    : result.ToProblem();
            })
            .WithName("BatchCreateArticles")
            .WithTags("Articles")
            .RequireAuthorization();
    }
}
