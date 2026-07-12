namespace ExampleApi.Application.Common;

/// <summary>
/// A slice of persisted rows returned by a repository query, together with the total
/// number of rows that matched the filter (before paging). Pure data — the presentation
/// wrapper (page/pageSize/hasNext …) is computed higher up.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
/// <param name="Items">The items on the requested page.</param>
/// <param name="TotalCount">The total number of matching rows.</param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);
