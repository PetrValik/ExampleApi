namespace ExampleApi.Models;

/// <summary>
/// The Article domain/persistence entity.
/// </summary>
public sealed class Article
{
    /// <summary>Primary key (identity).</summary>
    public int ArticleId { get; set; }

    /// <summary>Article name (1–64 chars).</summary>
    public required string Name { get; set; }

    /// <summary>Article description (1–2048 chars).</summary>
    public required string Description { get; set; }

    /// <summary>Optional category (≤64 chars).</summary>
    public string? Category { get; set; }

    /// <summary>Price (≥ 0).</summary>
    public decimal Price { get; set; }

    /// <summary>ISO 4217 currency code; required only when Price &gt; 0.</summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Optimistic-concurrency token backed by the PostgreSQL <c>xmin</c> system column.
    /// Changes automatically on every row modification.
    /// </summary>
    public uint RowVersion { get; set; }
}
