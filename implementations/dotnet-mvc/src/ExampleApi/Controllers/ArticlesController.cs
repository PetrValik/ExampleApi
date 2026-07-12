using ExampleApi.Dtos;
using ExampleApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExampleApi.Controllers;

/// <summary>
/// Article CRUD, listing and batch creation. All actions require a JWT bearer token.
/// Controllers stay thin: bind, delegate to <see cref="IArticleService"/>, shape the HTTP result.
/// Validation (400) is handled by the global FluentValidation filter; 404/409 by the global
/// exception filter — so these actions never branch on error states.
/// </summary>
[ApiController]
[Authorize]
[Route("api/articles")]
public sealed class ArticlesController(IArticleService articleService) : ControllerBase
{
    /// <summary>GET /api/articles — filtered, paged list.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ArticleResponse>>> List(
        [FromQuery(Name = "name")] string? name,
        [FromQuery(Name = "category")] string? category,
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "pageSize")] int? pageSize,
        CancellationToken cancellationToken)
    {
        var result = await articleService.ListAsync(name, category, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>GET /api/articles/{id} — single article or 404.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ArticleResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var article = await articleService.GetByIdAsync(id, cancellationToken);
        return Ok(article);
    }

    /// <summary>POST /api/articles — create; 201 with a Location header ending in the new id.</summary>
    [HttpPost]
    public async Task<ActionResult<ArticleResponse>> Create(
        [FromBody] ArticleRequest request, CancellationToken cancellationToken)
    {
        var created = await articleService.CreateAsync(request, cancellationToken);
        return Created($"/api/articles/{created.ArticleId}", created);
    }

    /// <summary>POST /api/articles-concurrent — batch create; 201 with the created articles in order.</summary>
    [HttpPost("/api/articles-concurrent")]
    public async Task<ActionResult<IReadOnlyList<ArticleResponse>>> CreateBatch(
        [FromBody] List<ArticleRequest> request, CancellationToken cancellationToken)
    {
        var created = await articleService.CreateBatchAsync(request, cancellationToken);
        return Created("/api/articles-concurrent", created);
    }

    /// <summary>PUT /api/articles/{id} — full update with optimistic concurrency (409 on stale row_version).</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ArticleResponse>> Update(
        int id, [FromBody] UpdateArticleRequest request, CancellationToken cancellationToken)
    {
        var updated = await articleService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>DELETE /api/articles/{id} — 204 on success, 404 when absent.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await articleService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
