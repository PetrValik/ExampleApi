using ExampleApi.Common.Endpoints;
using ExampleApi.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ExampleApi.Features.Articles.DeleteArticle;

/// <summary>
/// <c>DELETE /api/articles/{id}</c> — 204 on success, 404 when absent. Requires a bearer token.
/// </summary>
internal sealed class DeleteArticleEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/articles/{id:int}", async (
                int id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new DeleteArticleCommand(id), cancellationToken);

                return result.IsSuccess
                    ? Results.NoContent()
                    : result.ToProblem();
            })
            .WithName("DeleteArticle")
            .WithTags("Articles")
            .RequireAuthorization();
    }
}
