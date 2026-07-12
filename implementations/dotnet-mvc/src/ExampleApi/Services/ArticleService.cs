using ExampleApi.Common.Exceptions;
using ExampleApi.Dtos;
using ExampleApi.Mapping;
using ExampleApi.Repositories;

namespace ExampleApi.Services;

/// <summary>
/// Default <see cref="IArticleService"/> implementation. Contains the pagination-clamping and
/// optimistic-concurrency orchestration; delegates all persistence to <see cref="IArticleRepository"/>.
/// </summary>
public sealed class ArticleService(
    IArticleRepository repository,
    IServiceScopeFactory scopeFactory) : IArticleService
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    /// <inheritdoc />
    public async Task<ArticleResponse> CreateAsync(ArticleRequest request, CancellationToken cancellationToken)
    {
        var created = await repository.AddAsync(request.ToEntity(), cancellationToken);
        return created.ToResponse();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleResponse>> CreateBatchAsync(
        IReadOnlyList<ArticleRequest> requests, CancellationToken cancellationToken)
    {
        // Persist each item on its own DbContext scope so the inserts can run genuinely in
        // parallel (a DbContext is not thread-safe). Task.WhenAll preserves input order.
        var tasks = requests.Select(async request =>
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var scopedRepository = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
            var created = await scopedRepository.AddAsync(request.ToEntity(), cancellationToken);
            return created.ToResponse();
        });

        var results = await Task.WhenAll(tasks);
        return results;
    }

    /// <inheritdoc />
    public async Task<ArticleResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var article = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Article with ID {id} was not found.");
        return article.ToResponse();
    }

    /// <inheritdoc />
    public async Task<PagedResponse<ArticleResponse>> ListAsync(
        string? name, string? category, int? page, int? pageSize, CancellationToken cancellationToken)
    {
        var effectivePage = Math.Max(1, page ?? 1);
        var effectivePageSize = Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize);

        var (items, totalCount) = await repository.ListAsync(
            name, category, effectivePage, effectivePageSize, cancellationToken);

        return new PagedResponse<ArticleResponse>
        {
            Items = [.. items.Select(article => article.ToResponse())],
            Page = effectivePage,
            PageSize = effectivePageSize,
            TotalCount = totalCount,
        };
    }

    /// <inheritdoc />
    public async Task<ArticleResponse> UpdateAsync(int id, UpdateArticleRequest request, CancellationToken cancellationToken)
    {
        var article = await repository.GetTrackedByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Article with ID {id} was not found.");

        request.ApplyTo(article);

        // request.RowVersion is guaranteed non-null by the validator (400 before reaching here).
        var saved = await repository.SaveUpdateAsync(article, request.RowVersion ?? 0u, cancellationToken);
        if (!saved)
        {
            throw new ConflictException($"Article with ID {id} was modified by another request. Please retry.");
        }

        return article.ToResponse();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Article with ID {id} was not found.");
        }
    }
}
