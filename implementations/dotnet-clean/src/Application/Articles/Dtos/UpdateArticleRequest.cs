using System.Text.Json.Serialization;

namespace ExampleApi.Application.Articles.Dtos;

/// <summary>
/// The update request body. Identical to the create body plus a required
/// <c>row_version</c> optimistic-concurrency token (int64, ≥ 1).
/// </summary>
public sealed class UpdateArticleRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>
    /// The row version echoed from a prior read. Nullable so a missing value fails
    /// validation (400) rather than silently defaulting to zero.
    /// </summary>
    [JsonPropertyName("row_version")]
    public long? RowVersion { get; set; }
}
