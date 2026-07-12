using System.Text.Json.Serialization;

namespace ExampleApi.Features.Articles.Shared.Dtos;

/// <summary>
/// The update request body. Identical to <see cref="ArticleRequest"/> plus the required
/// <c>row_version</c> optimistic-concurrency token. Field names are snake_case.
/// </summary>
public sealed class UpdateArticleRequest
{
    /// <summary>The article name (required, 1–64 chars).</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>The description (required, 1–2048 chars).</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>The optional category (≤64 chars).</summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>The price (≥ 0).</summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>The ISO 4217 currency code (required only when price &gt; 0).</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>The row version from a prior GET (required, must be non-zero).</summary>
    [JsonPropertyName("row_version")]
    public uint? RowVersion { get; set; }
}
