using System.Text.Json.Serialization;

namespace ExampleApi.Application.Articles.Dtos;

/// <summary>
/// The pagination wrapper. Field names are camelCase per the contract (intentionally
/// inconsistent with the snake_case article items nested inside <see cref="Items"/>).
/// Property names are pinned explicitly so the shape does not depend on the ambient
/// serializer naming policy.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class PagedResponse<T>
{
    [JsonPropertyName("items")]
    public required IReadOnlyList<T> Items { get; init; }

    [JsonPropertyName("page")]
    public required int Page { get; init; }

    [JsonPropertyName("pageSize")]
    public required int PageSize { get; init; }

    [JsonPropertyName("totalCount")]
    public required int TotalCount { get; init; }

    [JsonPropertyName("totalPages")]
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    [JsonPropertyName("hasPrevious")]
    public bool HasPrevious => Page > 1;

    [JsonPropertyName("hasNext")]
    public bool HasNext => Page < TotalPages;

    /// <summary>
    /// Builds a page wrapper from the current page's items and the overall count.
    /// </summary>
    public static PagedResponse<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount) =>
        new()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
}
