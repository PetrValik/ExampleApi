namespace ExampleApi.Common.Pagination;

/// <summary>
/// Represents a paginated response with metadata.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public sealed class PagedResponse<T>
{
    /// <summary>
    /// Gets or sets the items for the current page.
    /// </summary>
    public required List<T> Items { get; set; }

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public required int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public required int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of items across all pages.
    /// </summary>
    public required int TotalCount { get; set; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNext => Page < TotalPages;
}
