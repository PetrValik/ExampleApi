using System.Text.Json.Serialization;

namespace ExampleApi.Dtos;

/// <summary>
/// Response body for an article. Field names are snake_case per the shared contract.
/// </summary>
public sealed class ArticleResponse
{
    [JsonPropertyName("article_id")]
    public required int ArticleId { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("price")]
    public required decimal Price { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("row_version")]
    public required uint RowVersion { get; set; }
}
