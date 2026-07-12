using System.Text.Json.Serialization;

namespace ExampleApi.Dtos;

/// <summary>
/// Request body for updating an article. Unlike the create request this also carries the
/// <c>row_version</c> optimistic-concurrency token, which is required.
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

    /// <summary>The row version obtained from a prior GET. Required and must be non-zero.</summary>
    [JsonPropertyName("row_version")]
    public uint? RowVersion { get; set; }
}
