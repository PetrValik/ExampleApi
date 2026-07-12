using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.Features.Articles.UpdateArticle;
using ExampleApi.IntegrationTests.Common;

namespace ExampleApi.IntegrationTests.Features.Articles.UpdateArticle;

/// <summary>
/// Integration tests for UpdateArticle endpoint (PUT /api/articles/{id}).
/// </summary>
public class UpdateArticleEndpointTests : IntegrationTestBase
{
    /// <summary>
    /// Creates an article via the API and returns the deserialized response.
    /// </summary>
    private async Task<ArticleResponse> CreateArticleAsync(string name)
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
    /// Valid update request returns 200 OK with the updated article body.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithValidData_ReturnsOkAndUpdatedArticle()
    {
        // Arrange - Create an article first
        var createdArticle = await CreateArticleAsync("Original Name");

        var updateRequest = new UpdateArticleRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 19.99m,
            Currency = "EUR",
            RowVersion = createdArticle.RowVersion
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/articles/{createdArticle.ArticleId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedArticle = await response.Content.ReadFromJsonAsync<ArticleResponse>();
        updatedArticle.Should().NotBeNull();
        updatedArticle!.Name.Should().Be("Updated Name");
        updatedArticle.Price.Should().Be(19.99m);
        updatedArticle.Currency.Should().Be("EUR");
    }

    /// <summary>
    /// Update request with a stale <c>RowVersion</c> returns 409 Conflict.
    /// </summary>
    [Fact]
    public async Task UpdateArticle_WithStaleRowVersion_ReturnsConflict()
    {
        // Arrange - Create and update an article to change RowVersion
        var createdArticle = await CreateArticleAsync("Test Product");

        // First update
        var firstUpdate = new UpdateArticleRequest
        {
            Name = "First Update",
            Description = "Updated",
            Price = 15.00m,
            Currency = "USD",
            RowVersion = createdArticle.RowVersion
        };
        await Client.PutAsJsonAsync($"/api/articles/{createdArticle.ArticleId}", firstUpdate);

        // Try to update with stale RowVersion
        var secondUpdate = new UpdateArticleRequest
        {
            Name = "Second Update",
            Description = "Updated Again",
            Price = 20.00m,
            Currency = "USD",
            RowVersion = createdArticle.RowVersion // Stale version
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/articles/{createdArticle.ArticleId}", secondUpdate);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
