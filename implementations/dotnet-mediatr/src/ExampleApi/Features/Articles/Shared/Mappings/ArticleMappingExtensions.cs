using ExampleApi.Features.Articles.Shared.Dtos;
using ExampleApi.Features.Articles.Shared.Models;

namespace ExampleApi.Features.Articles.Shared.Mappings;

/// <summary>
/// Mapping between the <see cref="Article"/> entity and its wire DTOs.
/// </summary>
public static class ArticleMappingExtensions
{
    /// <summary>Projects an entity onto its response DTO.</summary>
    public static ArticleResponse ToResponse(this Article article) => new()
    {
        ArticleId = article.ArticleId,
        Name = article.Name,
        Description = article.Description,
        Category = article.Category,
        Price = article.Price,
        Currency = article.Currency,
        RowVersion = article.RowVersion
    };

    /// <summary>Builds a new entity from a create request.</summary>
    public static Article ToEntity(this ArticleRequest request) => new()
    {
        Name = request.Name,
        Description = request.Description,
        Category = request.Category,
        Price = request.Price,
        Currency = request.Currency
    };
}
