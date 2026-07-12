using ExampleApi.Common.Endpoints;
using ExampleApi.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ExampleApi.Features.Articles.GetArticles;

/// <summary>
/// <c>GET /api/articles</c> — filtered, paginated listing. Requires a bearer token.
/// </summary>
internal sealed class GetArticlesEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/articles", async (
                string? name,
                string? category,
                int? page,
                int? pageSize,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetArticlesQuery(name, category, page, pageSize), cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToProblem();
            })
            .WithName("GetArticles")
            .WithTags("Articles")
            .RequireAuthorization();
    }
}
