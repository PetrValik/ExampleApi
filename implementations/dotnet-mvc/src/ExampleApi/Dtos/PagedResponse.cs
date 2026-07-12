namespace ExampleApi.Dtos;

/// <summary>
/// A page of results plus pagination metadata. The wrapper uses camelCase field names
/// (the default naming policy), intentionally different from the snake_case article items.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class PagedResponse<T>
{
    public required List<T> Items { get; set; }

    public required int Page { get; set; }

    public required int PageSize { get; set; }

    public required int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;
}
