using FluentValidation;
using ExampleApi.Common.Endpoints;
using ExampleApi.Common.Validation;
using ExampleApi.Features.Articles.Shared.DTOs;

namespace ExampleApi.Features.Articles.BatchCreateArticles;

/// <summary>
/// Endpoint for creating multiple articles concurrently.
/// </summary>
public sealed class BatchCreateArticlesEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/articles-concurrent", async (
                List<ArticleRequest> request,
                IValidator<List<ArticleRequest>> validator,
                IBatchCreateArticlesHandler handler,
                CancellationToken cancellationToken) =>
            {
                var validationResult = await ValidationFilter.ValidateAsync(validator, request, cancellationToken);
                if (validationResult is not null)
                {
                    return validationResult;
                }

                var response = await handler.HandleAsync(request, cancellationToken);
                return Results.Created("/api/articles", response);
            })
            .WithName("BatchCreateArticles")
            .WithTags("Articles")
            .WithSummary("Create multiple articles concurrently")
            .WithDescription("Creates multiple articles in parallel. Returns the list of created articles with generated IDs.")
            .Produces<List<ArticleResponse>>(StatusCodes.Status201Created, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }
}
