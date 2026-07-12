using ExampleApi.Common.Endpoints;
using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ExampleApi.Features.Articles.CreateArticle;

/// <summary>
/// <c>POST /api/articles</c> — creates an article. 201 + Location on success, 400 on
/// validation failure. Requires a bearer token.
/// </summary>
internal sealed class CreateArticleEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/articles", async (
                ArticleRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new CreateArticleCommand(request), cancellationToken);

                return result.IsSuccess
                    ? Results.Created($"/api/articles/{result.Value.ArticleId}", result.Value)
                    : result.ToProblem();
            })
            .WithName("CreateArticle")
            .WithTags("Articles")
            .RequireAuthorization();
    }
}
