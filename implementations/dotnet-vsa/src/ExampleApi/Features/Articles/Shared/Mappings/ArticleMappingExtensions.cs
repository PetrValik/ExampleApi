using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.Features.Articles.Shared.Models;

namespace ExampleApi.Features.Articles.Shared.Mappings;

/// <summary>
/// Provides extension methods for mapping Article entities to response models.
/// </summary>
public static class ArticleMappingExtensions
{
    /// <summary>
    /// Maps an Article entity to an ArticleResponse.
    /// </summary>
    /// <param name="article">The article entity to map.</param>
    /// <returns>The mapped article response.</returns>
    public static ArticleResponse ToResponse(this Article article)
    {
        return new ArticleResponse
        {
            ArticleId = article.ArticleId,
            Name = article.Name,
            Description = article.Description,
            Category = article.Category,
            Price = article.Price,
            Currency = article.Currency,
            RowVersion = article.RowVersion
        };
    }
}
