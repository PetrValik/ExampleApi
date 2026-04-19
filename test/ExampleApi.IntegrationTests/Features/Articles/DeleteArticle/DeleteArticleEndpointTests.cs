using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.IntegrationTests.Common;

namespace ExampleApi.IntegrationTests.Features.Articles.DeleteArticle;

/// <summary>
/// Integration tests for DeleteArticle endpoint (DELETE /api/articles/{id}).
/// </summary>
public class DeleteArticleEndpointTests : IntegrationTestBase
{
    /// <summary>
    /// Creates an article via the API and returns the deserialized response.
    /// </summary>
    private async Task<ArticleResponse> CreateArticleAsync(string name = "Test Article")
    {
        var request = new ArticleRequest
        {
            Name = name,
            Description = "Test Description",
            Price = 9.99m,
            Currency = "USD"
        };
        var response = await Client.PostAsJsonAsync("/api/articles", request);
        return (await response.Content.ReadFromJsonAsync<ArticleResponse>())!;
    }

    /// <summary>
    /// Existing article ID returns 204 No Content.
    /// </summary>
    [Fact]
    public async Task DeleteArticle_WithExistingId_ReturnsNoContent()
    {
        // Arrange
        var article = await CreateArticleAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/articles/{article.ArticleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// After deletion the article is no longer retrievable and GET returns 404.
    /// </summary>
    [Fact]
    public async Task DeleteArticle_WithExistingId_ArticleIsNoLongerRetrievable()
    {
        // Arrange
        var article = await CreateArticleAsync();

        // Act
        await Client.DeleteAsync($"/api/articles/{article.ArticleId}");

        // Assert
        var getResponse = await Client.GetAsync($"/api/articles/{article.ArticleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Non-existent article ID returns 404 Not Found.
    /// </summary>
    [Fact]
    public async Task DeleteArticle_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await Client.DeleteAsync("/api/articles/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}