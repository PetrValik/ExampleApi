using System.Text.Json.Serialization;

namespace ExampleApi.Features.Articles.Shared.DTOs;

/// <summary>
/// Represents the response model for an article.
/// </summary>
public sealed class ArticleResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the article.
    /// </summary>
    [JsonPropertyName("article_id")]
    public required int ArticleId { get; set; }

    /// <summary>
    /// Gets or sets the name of the article.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the article.
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the category of the article.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the price of the article.
    /// </summary>
    [JsonPropertyName("price")]
    public required decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the currency code (ISO 4217) for the article price.
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// </summary>
    [JsonPropertyName("row_version")]
    public uint RowVersion { get; set; }
}
