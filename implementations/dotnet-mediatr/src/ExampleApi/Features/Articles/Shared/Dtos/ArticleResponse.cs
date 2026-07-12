using System.Text.Json.Serialization;

namespace ExampleApi.Features.Articles.Shared.Dtos;

/// <summary>
/// The article response body. Field names are snake_case per the contract.
/// </summary>
public sealed class ArticleResponse
{
    /// <summary>The article identifier.</summary>
    [JsonPropertyName("article_id")]
    public required int ArticleId { get; init; }

    /// <summary>The article name.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>The description.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>The optional category.</summary>
    [JsonPropertyName("category")]
    public string? Category { get; init; }

    /// <summary>The price.</summary>
    [JsonPropertyName("price")]
    public required decimal Price { get; init; }

    /// <summary>The optional ISO 4217 currency code.</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    /// <summary>The optimistic-concurrency token (xmin-backed).</summary>
    [JsonPropertyName("row_version")]
    public required uint RowVersion { get; init; }
}
