using ExampleApi.Common.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace ExampleApi.Features.Articles.DeleteArticle;

/// <summary>
/// Endpoint for deleting an article.
/// </summary>
public sealed class DeleteArticleEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/articles/{id:int}", async (
            int id,
            IDeleteArticleHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(id, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteArticle")
        .WithTags("Articles")
        .WithSummary("Delete an article")
        .WithDescription("Permanently deletes an article by its ID.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")
        .RequireAuthorization();
    }
}