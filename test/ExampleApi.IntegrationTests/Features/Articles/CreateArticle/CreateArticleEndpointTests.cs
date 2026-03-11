using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.IntegrationTests.Common;

namespace ExampleApi.IntegrationTests.Features.Articles.CreateArticle;

/// <summary>
/// Integration tests for CreateArticle endpoint (POST /api/articles).
/// </summary>
public class CreateArticleEndpointTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateArticle_WithValidData_ReturnsOkAndArticleWithId()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Integration Test Product",
            Description = "Test Description",
            Price = 9.99m,
            Currency = "USD"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/articles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var article = await response.Content.ReadFromJsonAsync<ArticleResponse>();
        article.Should().NotBeNull();
        article!.ArticleId.Should().BeGreaterThan(0);
        article.Name.Should().Be("Integration Test Product");
        article.Description.Should().Be("Test Description");
        article.Price.Should().Be(9.99m);
        article.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task CreateArticle_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "", // Invalid: empty name
            Description = "Description",
            Price = 10.00m,
            Currency = "USD"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/articles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateArticle_WithFreePrice_DoesNotRequireCurrency()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Free Product",
            Description = "Free item",
            Price = 0,
            Currency = null
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/articles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var article = await response.Content.ReadFromJsonAsync<ArticleResponse>();
        article.Should().NotBeNull();
        article!.Price.Should().Be(0);
        article.Currency.Should().BeNull();
    }

    [Fact]
    public async Task CreateArticle_WithPriceButNoCurrency_ReturnsBadRequest()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Product",
            Description = "Description",
            Price = 10.00m,
            Currency = null // Invalid: price > 0 but no currency
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/articles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
