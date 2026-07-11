using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.IntegrationTests.Common;

namespace ExampleApi.IntegrationTests.Features.Articles.BatchCreateArticles;

/// <summary>
/// Integration tests for BatchCreateArticles endpoint (POST /api/articles-concurrent).
/// </summary>
public class BatchCreateArticlesEndpointTests : IntegrationTestBase
{
    /// <summary>
    /// Builds a valid <see cref="ArticleRequest"/> with the given name and fixed test values.
    /// </summary>
    private static ArticleRequest CreateValidArticleRequest(string name) => new()
    {
        Name = name,
        Description = "Test Description",
        Price = 9.99m,
        Currency = "USD"
    };

    /// <summary>
    /// Expected product names used to verify the batch create response.
    /// </summary>
    private static readonly string[] ExpectedProductNames = ["Batch Product 1", "Batch Product 2", "Batch Product 3"];

    /// <summary>
    /// Valid list of three articles returns 201 Created with all articles in the response body.
    /// </summary>
    [Fact]
    public async Task BatchCreateArticles_WithValidData_ReturnsOkAndAllArticles()
    {
        // Arrange
        ArticleRequest[] requests =
        [
            CreateValidArticleRequest("Batch Product 1"),
            CreateValidArticleRequest("Batch Product 2"),
            CreateValidArticleRequest("Batch Product 3")
        ];

        // Act
        var response = await Client.PostAsJsonAsync("/api/articles-concurrent", requests);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var articles = await response.Content.ReadFromJsonAsync<List<ArticleResponse>>();
        articles.Should().NotBeNull();
        articles!.Should().HaveCount(3);
        articles.Should().OnlyContain(a => a.ArticleId > 0);
        articles!.Select(a => a.Name).Should().Contain(ExpectedProductNames);
    }

    /// <summary>
    /// List containing an article with an empty name returns 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task BatchCreateArticles_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        ArticleRequest[] requests =
        [
            CreateValidArticleRequest("Valid Product"),
            new()
            {
                Name = "", // Invalid
                Description = "Description",
                Price = 10.00m,
                Currency = "USD"
            }
        ];

        // Act
        var response = await Client.PostAsJsonAsync("/api/articles-concurrent", requests);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
