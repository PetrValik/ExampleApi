using ExampleApi.Common.Endpoints;
using ExampleApi.Features.Articles.Shared.DTOs;

namespace ExampleApi.Features.Articles.GetArticle;

/// <summary>
/// Endpoint for retrieving a single article by ID.
/// </summary>
public sealed class GetArticleEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/articles/{articleId:int}", async (
            int articleId,
            IGetArticleHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(articleId, cancellationToken);
            return Results.Ok(response);
        })
        .WithName("GetArticle")
        .WithTags("Articles")
        .WithSummary("Get article by ID")
        .WithDescription("Retrieves a single article by its unique identifier.")
        .Produces<ArticleResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
