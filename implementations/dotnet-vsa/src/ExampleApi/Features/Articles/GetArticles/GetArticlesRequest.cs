using Microsoft.AspNetCore.Mvc;

namespace ExampleApi.Features.Articles.GetArticles;

/// <summary>
/// Represents a request to get articles with optional filters and pagination.
/// </summary>
public sealed class GetArticlesRequest
{
    /// <summary>
    /// Gets or sets the name filter (partial match, case-insensitive).
    /// </summary>
    [FromQuery(Name = "name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the category filter (exact match).
    /// </summary>
    [FromQuery(Name = "category")]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the page number (1-based). Default is 1.
    /// </summary>
    [FromQuery(Name = "page")]
    public int? Page { get; set; }

    /// <summary>
    /// Gets or sets the page size. Default is 10. Maximum is 100.
    /// </summary>
    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }
}
