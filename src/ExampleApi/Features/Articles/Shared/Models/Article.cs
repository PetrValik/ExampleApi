namespace ExampleApi.Features.Articles.Shared.Models;

/// <summary>
/// Represents a shop article entity.
/// </summary>
public sealed class Article
{
    /// <summary>
    /// Gets or sets the unique identifier of the article.
    /// </summary>
    public int ArticleId { get; set; }

    /// <summary>
    /// Gets or sets the name of the article.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the article.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the category of the article.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the price of the article.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the currency code (ISO 4217) for the article price.
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// </summary>
    public byte[]? RowVersion { get; set; }
}
