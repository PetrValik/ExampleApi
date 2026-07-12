using System.Text.Json.Serialization;

namespace ExampleApi.Features.Articles.Shared.Dtos;

/// <summary>
/// The create (and batch-create) request body. Field names are snake_case per the contract.
/// </summary>
public sealed class ArticleRequest
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
}
