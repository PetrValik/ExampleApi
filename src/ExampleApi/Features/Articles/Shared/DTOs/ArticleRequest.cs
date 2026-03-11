using System.Text.Json.Serialization;

namespace ExampleApi.Features.Articles.Shared.DTOs;

/// <summary>
/// Represents the input model for creating or batch-creating an article.
/// </summary>
public sealed class ArticleRequest
{
    /// <summary>
    /// Gets or sets the name of the article.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the article.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category of the article.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the price of the article.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the currency code (ISO 4217) for the article price.
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}
