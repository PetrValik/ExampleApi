using FluentValidation;
using ExampleApi.Common.Endpoints;
using ExampleApi.Common.Validation;
using ExampleApi.Features.Articles.Shared.DTOs;

namespace ExampleApi.Features.Articles.CreateArticle;

/// <summary>
/// Endpoint for creating a new article.
/// </summary>
public sealed class CreateArticleEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/articles", async (
            ArticleRequest request,
            IValidator<ArticleRequest> validator,
            ICreateArticleHandler handler,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await ValidationFilter.ValidateAsync(validator, request, cancellationToken);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var response = await handler.HandleAsync(request, cancellationToken);
            return Results.Created($"/api/articles/{response.ArticleId}", response);
        })
        .WithName("CreateArticle")
        .WithTags("Articles")
        .WithSummary("Create a new article")
        .WithDescription("Creates a new article with the provided details. Returns the created article with a generated ID.")
        .Produces<ArticleResponse>(StatusCodes.Status201Created, "application/json")
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization();
    }
}
