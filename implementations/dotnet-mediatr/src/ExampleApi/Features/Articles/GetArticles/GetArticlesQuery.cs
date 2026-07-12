using ExampleApi.Common.Pagination;
using ExampleApi.Common.Results;
using ExampleApi.Features.Articles.Shared.Dtos;
using MediatR;

namespace ExampleApi.Features.Articles.GetArticles;

/// <summary>
/// Lists articles with optional name (partial, case-insensitive) and category (exact)
/// filters plus pagination (pageSize clamped to 1..100).
/// </summary>
/// <param name="Name">Optional partial, case-insensitive name filter.</param>
/// <param name="Category">Optional exact category filter.</param>
/// <param name="Page">Optional 1-based page number (default 1).</param>
/// <param name="PageSize">Optional page size (default 10, clamped to 100).</param>
public sealed record GetArticlesQuery(string? Name, string? Category, int? Page, int? PageSize)
    : IRequest<Result<PagedResponse<ArticleResponse>>>;
