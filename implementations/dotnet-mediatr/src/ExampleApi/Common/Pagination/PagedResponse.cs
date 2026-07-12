namespace ExampleApi.Common.Pagination;

/// <summary>
/// A page of results plus pagination metadata. Serialized with camelCase keys
/// (<c>items</c>, <c>pageSize</c>, <c>totalCount</c>, …) per the contract.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class PagedResponse<T>
{
    /// <summary>The items on the current page.</summary>
    public required List<T> Items { get; init; }

    /// <summary>The 1-based page number.</summary>
    public required int Page { get; init; }

    /// <summary>The page size actually applied (clamped to 1..100).</summary>
    public required int PageSize { get; init; }

    /// <summary>The total number of matching items across all pages.</summary>
    public required int TotalCount { get; init; }

    /// <summary>The total number of pages.</summary>
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Whether a previous page exists.</summary>
    public bool HasPrevious => Page > 1;

    /// <summary>Whether a next page exists.</summary>
    public bool HasNext => Page < TotalPages;
}
