using System.Text.Json.Serialization;

namespace ExampleApi.Dtos;

/// <summary>
/// Request body for creating an article (also used for each item of a batch create).
/// Field names are snake_case per the shared contract.
/// </summary>
public sealed class ArticleRequest
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
}
