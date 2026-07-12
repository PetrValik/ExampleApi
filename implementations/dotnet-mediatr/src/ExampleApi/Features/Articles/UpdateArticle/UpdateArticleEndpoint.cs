using ExampleApi.Common.Endpoints;
using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ExampleApi.Features.Articles.UpdateArticle;

/// <summary>
/// <c>PUT /api/articles/{id}</c> — replaces an article. 200 on success, 400/404/409 on
/// validation / missing / stale-version. Requires a bearer token.
/// </summary>
internal sealed class UpdateArticleEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/articles/{id:int}", async (
                int id,
                UpdateArticleRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new UpdateArticleCommand(id, request), cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToProblem();
            })
            .WithName("UpdateArticle")
            .WithTags("Articles")
            .RequireAuthorization();
    }
}
