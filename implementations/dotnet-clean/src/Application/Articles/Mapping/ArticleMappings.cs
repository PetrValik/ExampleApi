using ExampleApi.Application.Articles.Dtos;
using ExampleApi.Domain.Entities;

namespace ExampleApi.Application.Articles.Mapping;

/// <summary>
/// Maps <see cref="Article"/> aggregates to their wire responses.
/// </summary>
public static class ArticleMappings
{
    /// <summary>
    /// Projects an article entity onto an <see cref="ArticleResponse"/>.
    /// </summary>
    public static ArticleResponse ToResponse(this Article article) =>
        new()
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
