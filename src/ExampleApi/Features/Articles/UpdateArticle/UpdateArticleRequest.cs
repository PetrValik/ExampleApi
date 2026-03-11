using System.Text.Json.Serialization;

namespace ExampleApi.Features.Articles.UpdateArticle;

/// <summary>
/// Represents a request to update an existing article.
/// </summary>
public sealed class UpdateArticleRequest
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

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// The client must send the row version received from a previous GET request.
    /// </summary>
    [JsonPropertyName("row_version")]
    public byte[]? RowVersion { get; set; }
}
