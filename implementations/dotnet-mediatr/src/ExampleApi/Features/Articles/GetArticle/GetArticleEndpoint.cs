using ExampleApi.Common.Endpoints;
using ExampleApi.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ExampleApi.Features.Articles.GetArticle;

/// <summary>
/// <c>GET /api/articles/{id}</c> — single article, 404 when absent. Requires a bearer token.
/// </summary>
internal sealed class GetArticleEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/articles/{id:int}", async (
                int id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetArticleQuery(id), cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToProblem();
            })
            .WithName("GetArticle")
            .WithTags("Articles")
            .RequireAuthorization();
    }
}
