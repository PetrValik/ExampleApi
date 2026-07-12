using System.Text.Json.Serialization;

namespace ExampleApi.Application.Articles.Dtos;

/// <summary>
/// The article response body (snake_case field names per the contract).
/// </summary>
public sealed class ArticleResponse
{
    [JsonPropertyName("article_id")]
    public required int ArticleId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("price")]
    public required decimal Price { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("row_version")]
    public required long RowVersion { get; init; }
}
