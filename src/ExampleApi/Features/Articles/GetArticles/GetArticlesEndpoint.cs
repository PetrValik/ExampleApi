using ExampleApi.Common.Endpoints;
using ExampleApi.Common.Pagination;
using ExampleApi.Features.Articles.Shared.DTOs;

namespace ExampleApi.Features.Articles.GetArticles;

/// <summary>
/// Endpoint for retrieving articles with optional filters and pagination.
/// </summary>
public sealed class GetArticlesEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/articles", async (
            [AsParameters] GetArticlesRequest request,
            IGetArticlesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(request, cancellationToken);
            return Results.Ok(response);
        })
        .WithName("GetArticles")
        .WithTags("Articles")
        .WithSummary("Get articles with pagination")
        .WithDescription("Retrieves a paginated list of articles. Supports filtering by name (partial match, case-insensitive) and category (exact match). Default page size is 10, maximum is 100.")
        .Produces<PagedResponse<ArticleResponse>>(StatusCodes.Status200OK);
    }
}
