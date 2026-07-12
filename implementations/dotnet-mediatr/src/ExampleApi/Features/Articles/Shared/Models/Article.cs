namespace ExampleApi.Features.Articles.Shared.Models;

/// <summary>
/// The article persistence entity.
/// </summary>
public sealed class Article
{
    /// <summary>The generated identifier.</summary>
    public int ArticleId { get; set; }

    /// <summary>The article name.</summary>
    public required string Name { get; set; }

    /// <summary>The article description.</summary>
    public required string Description { get; set; }

    /// <summary>The optional category.</summary>
    public string? Category { get; set; }

    /// <summary>The price.</summary>
    public decimal Price { get; set; }

    /// <summary>The optional ISO 4217 currency code.</summary>
    public string? Currency { get; set; }

    /// <summary>
    /// The optimistic-concurrency token, mapped to the PostgreSQL <c>xmin</c> system column.
    /// </summary>
    public uint RowVersion { get; set; }
}
