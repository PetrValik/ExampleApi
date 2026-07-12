using ExampleApi.Application.Articles.Dtos;
using ExampleApi.Application.Articles.UseCases;
using FluentValidation;

namespace ExampleApi.WebApi.Endpoints;

/// <summary>
/// The article resource: CRUD, listing and batch creation. Every route requires a
/// valid bearer token.
/// </summary>
public static class ArticleEndpoints
{
    public static IEndpointRouteBuilder MapArticleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/articles")
            .WithTags("Articles")
            .RequireAuthorization();

        // GET /api/articles — filter + pagination
        group.MapGet("", async (
            string? name,
            string? category,
            int? page,
            int? pageSize,
            IGetArticlesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(new GetArticlesQuery(name, category, page, pageSize), cancellationToken);
            return Results.Ok(response);
        })
        .WithName("GetArticles");

        // GET /api/articles/{id}
        group.MapGet("/{id:int}", async (
            int id,
            IGetArticleHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(id, cancellationToken);
            return Results.Ok(response);
        })
        .WithName("GetArticle");

        // POST /api/articles
        group.MapPost("", async (
            CreateArticleRequest request,
            IValidator<CreateArticleRequest> validator,
            ICreateArticleHandler handler,
            CancellationToken cancellationToken) =>
        {
            var validationProblem = await EndpointValidation.ValidateAsync(validator, request, cancellationToken);
            if (validationProblem is not null)
            {
                return validationProblem;
            }

            var response = await handler.HandleAsync(request, cancellationToken);
            return Results.Created($"/api/articles/{response.ArticleId}", response);
        })
        .WithName("CreateArticle");

        // PUT /api/articles/{id}
        group.MapPut("/{id:int}", async (
            int id,
            UpdateArticleRequest request,
            IValidator<UpdateArticleRequest> validator,
            IUpdateArticleHandler handler,
            CancellationToken cancellationToken) =>
        {
            var validationProblem = await EndpointValidation.ValidateAsync(validator, request, cancellationToken);
            if (validationProblem is not null)
            {
                return validationProblem;
            }

            var response = await handler.HandleAsync(id, request, cancellationToken);
            return Results.Ok(response);
        })
        .WithName("UpdateArticle");

        // DELETE /api/articles/{id}
        group.MapDelete("/{id:int}", async (
            int id,
            IDeleteArticleHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(id, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteArticle");

        // POST /api/articles-concurrent — batch creation (sibling route, still protected)
        app.MapPost("/api/articles-concurrent", async (
            List<CreateArticleRequest> request,
            IValidator<List<CreateArticleRequest>> validator,
            IBatchCreateArticlesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var validationProblem = await EndpointValidation.ValidateAsync(validator, request, cancellationToken);
            if (validationProblem is not null)
            {
                return validationProblem;
            }

            var response = await handler.HandleAsync(request, cancellationToken);
            return Results.Created("/api/articles", response);
        })
        .WithName("BatchCreateArticles")
        .WithTags("Articles")
        .RequireAuthorization();

        return app;
    }
}
