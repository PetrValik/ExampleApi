using FluentValidation;
using ExampleApi.Common.Endpoints;
using ExampleApi.Common.Validation;
using ExampleApi.Features.Articles.Shared.DTOs;

namespace ExampleApi.Features.Articles.UpdateArticle;

/// <summary>
/// Endpoint for updating an existing article.
/// </summary>
public sealed class UpdateArticleEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/articles/{id}", async (
            int id,
            UpdateArticleRequest request,
            IValidator<UpdateArticleRequest> validator,
            IUpdateArticleHandler handler,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await ValidationFilter.ValidateAsync(validator, request, cancellationToken);
            if (validationResult is not null)
            {
                return validationResult;
            }

            var response = await handler.HandleAsync(id, request, cancellationToken);
            return Results.Ok(response);
        })
        .WithName("UpdateArticle")
        .WithTags("Articles")
        .WithSummary("Update an existing article")
        .WithDescription("Updates all fields of an existing article by ID. Returns the updated article.")
        .Produces<ArticleResponse>(StatusCodes.Status200OK, "application/json")
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
        .RequireAuthorization();
    }
}
