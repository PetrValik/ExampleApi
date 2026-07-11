using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.IntegrationTests.Common;

namespace ExampleApi.IntegrationTests.Features.Articles.GetArticle;

/// <summary>
/// Integration tests for GetArticle endpoint (GET /api/articles/{id}).
/// </summary>
public class GetArticleEndpointTests : IntegrationTestBase
{
    /// <summary>
    /// Existing article returns 200 OK with the correct article body.
    /// </summary>
    [Fact]
    public async Task GetArticle_WhenExists_ReturnsOkAndArticle()
    {
        // Arrange - Create an article first
        var createRequest = new ArticleRequest
        {
            Name = "Get Test Product",
            Description = "Test Description",
            Price = 9.99m,
            Currency = "USD"
        };
        var createResponse = await Client.PostAsJsonAsync("/api/articles", createRequest);
        var createdArticle = await createResponse.Content.ReadFromJsonAsync<ArticleResponse>();

        // Act - Get the article
        var response = await Client.GetAsync($"/api/articles/{createdArticle!.ArticleId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var article = await response.Content.ReadFromJsonAsync<ArticleResponse>();
        article.Should().NotBeNull();
        article!.ArticleId.Should().Be(createdArticle.ArticleId);
        article.Name.Should().Be("Get Test Product");
    }

    /// <summary>
    /// Non-existent article ID returns 404 Not Found.
    /// </summary>
    [Fact]
    public async Task GetArticle_WhenNotExists_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync("/api/articles/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
