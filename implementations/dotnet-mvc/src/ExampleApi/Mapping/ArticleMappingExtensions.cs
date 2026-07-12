using ExampleApi.Dtos;
using ExampleApi.Models;

namespace ExampleApi.Mapping;

/// <summary>
/// Manual mapping between <see cref="Article"/> entities and the article DTOs.
/// Manual (rather than AutoMapper) to keep the dependency surface small and the mapping explicit.
/// </summary>
public static class ArticleMappingExtensions
{
    /// <summary>Maps an entity to its response DTO.</summary>
    public static ArticleResponse ToResponse(this Article article) => new()
    {
        ArticleId = article.ArticleId,
        Name = article.Name,
        Description = article.Description,
        Category = article.Category,
        Price = article.Price,
        Currency = article.Currency,
        RowVersion = article.RowVersion,
    };

    /// <summary>Builds a new entity from a create request.</summary>
    public static Article ToEntity(this ArticleRequest request) => new()
    {
        Name = request.Name,
        Description = request.Description,
        Category = request.Category,
        Price = request.Price,
        Currency = request.Currency,
    };

    /// <summary>Copies mutable fields from an update request onto an existing tracked entity.</summary>
    public static void ApplyTo(this UpdateArticleRequest request, Article article)
    {
        article.Name = request.Name;
        article.Description = request.Description;
        article.Category = request.Category;
        article.Price = request.Price;
        article.Currency = request.Currency;
    }
}
