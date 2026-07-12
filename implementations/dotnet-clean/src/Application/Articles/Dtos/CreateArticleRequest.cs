using System.Text.Json.Serialization;

namespace ExampleApi.Application.Articles.Dtos;

/// <summary>
/// The create/batch-create request body. Field names are snake_case per the contract.
/// <c>row_version</c> is deliberately absent here — it belongs only to updates.
/// </summary>
public sealed class CreateArticleRequest
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
