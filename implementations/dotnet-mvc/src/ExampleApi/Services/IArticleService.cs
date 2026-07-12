using ExampleApi.Dtos;

namespace ExampleApi.Services;

/// <summary>
/// Business logic for articles. Orchestrates the repository layer and maps to/from DTOs.
/// Throws <see cref="Common.Exceptions.NotFoundException"/> / <see cref="Common.Exceptions.ConflictException"/>
/// which the global exception filter translates into problem+json responses.
/// </summary>
public interface IArticleService
{
    Task<ArticleResponse> CreateAsync(ArticleRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<ArticleResponse>> CreateBatchAsync(IReadOnlyList<ArticleRequest> requests, CancellationToken cancellationToken);

    Task<ArticleResponse> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<PagedResponse<ArticleResponse>> ListAsync(
        string? name, string? category, int? page, int? pageSize, CancellationToken cancellationToken);

    Task<ArticleResponse> UpdateAsync(int id, UpdateArticleRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
