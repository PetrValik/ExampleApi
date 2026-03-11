using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExampleApi.Common.Pagination;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.IntegrationTests.Common;

namespace ExampleApi.IntegrationTests.Features.Articles.GetArticles;

/// <summary>
/// Integration tests for GetArticles endpoint (GET /api/articles).
/// </summary>
public class GetArticlesEndpointTests : IntegrationTestBase
{
    private static ArticleRequest CreateValidArticleRequest(string name) => new()
    {
        Name = name,
        Description = "Test Description",
        Price = 9.99m,
        Currency = "USD"
    };

    [Fact]
    public async Task GetArticles_ReturnsAllArticles()
    {
        // Arrange - Create multiple articles
        await Client.PostAsJsonAsync("/api/articles", CreateValidArticleRequest("Product A"));
        await Client.PostAsJsonAsync("/api/articles", CreateValidArticleRequest("Product B"));

        // Act
        var response = await Client.GetAsync("/api/articles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().HaveCount(2);
        pagedResponse.Items.Should().Contain(a => a.Name == "Product A");
        pagedResponse.Items.Should().Contain(a => a.Name == "Product B");
        pagedResponse.Page.Should().Be(1);
        pagedResponse.PageSize.Should().Be(10);
        pagedResponse.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetArticles_WithNameFilter_ReturnsFilteredArticles()
    {
        // Arrange - Create articles with different names
        await Client.PostAsJsonAsync("/api/articles", CreateValidArticleRequest("Memory Stick"));
        await Client.PostAsJsonAsync("/api/articles", CreateValidArticleRequest("Pen"));

        // Act
        var response = await Client.GetAsync("/api/articles?name=Memory");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().HaveCount(1);
        pagedResponse.Items.Should().Contain(a => a.Name.Contains("Memory"));
        pagedResponse.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetArticles_WithCategoryFilter_ReturnsOnlyMatchingCategory()
    {
        // Arrange
        var electronicRequest = CreateValidArticleRequest("Electronic Product");
        electronicRequest.Category = "Electronics";
        await Client.PostAsJsonAsync("/api/articles", electronicRequest);

        var stationary = CreateValidArticleRequest("Stationary Product");
        stationary.Category = "Stationary";
        await Client.PostAsJsonAsync("/api/articles", stationary);

        // Act
        var response = await Client.GetAsync("/api/articles?category=Electronics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().Contain(a => a.Category == "Electronics");
        pagedResponse.Items.Where(a => a.Category != null).Should().NotContain(a => a.Category == "Stationary");
        pagedResponse.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetArticles_WithPagination_ReturnsCorrectPage()
    {
        // Arrange - Create 15 articles
        for (int i = 1; i <= 15; i++)
        {
            await Client.PostAsJsonAsync("/api/articles", CreateValidArticleRequest($"Product {i}"));
        }

        // Act - Get page 2 with 5 items per page
        var response = await Client.GetAsync("/api/articles?page=2&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().HaveCount(5);
        pagedResponse.Page.Should().Be(2);
        pagedResponse.PageSize.Should().Be(5);
        pagedResponse.TotalCount.Should().Be(15);
        pagedResponse.TotalPages.Should().Be(3);
        pagedResponse.HasPrevious.Should().BeTrue();
        pagedResponse.HasNext.Should().BeTrue();
    }

    [Fact]
    public async Task GetArticles_WithPageSizeExceedingMax_ClampsToMaximum()
    {
        // Arrange - Create 5 articles
        for (int i = 1; i <= 5; i++)
        {
            await Client.PostAsJsonAsync("/api/articles", CreateValidArticleRequest($"Product {i}"));
        }

        // Act - Request page size of 200 (should be clamped to 100)
        var response = await Client.GetAsync("/api/articles?pageSize=200");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.PageSize.Should().Be(100); // Clamped to max
        pagedResponse.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetArticles_WithInvalidPageNumber_DefaultsToPageOne()
    {
        // Arrange
        await Client.PostAsJsonAsync("/api/articles", CreateValidArticleRequest("Product"));

        // Act - Request page 0 or negative
        var response = await Client.GetAsync("/api/articles?page=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Page.Should().Be(1); // Defaults to 1
    }

    [Fact]
    public async Task GetArticles_WithNoArticles_ReturnsEmptyList()
    {
        // Arrange - No articles created

        // Act
        var response = await Client.GetAsync("/api/articles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().BeEmpty();
        pagedResponse.TotalCount.Should().Be(0);
        pagedResponse.TotalPages.Should().Be(0);
        pagedResponse.HasPrevious.Should().BeFalse();
        pagedResponse.HasNext.Should().BeFalse();
    }

    [Fact]
    public async Task GetArticles_WithNameFilterNoMatches_ReturnsEmptyList()
    {
        // Arrange
        await Client.PostAsJsonAsync("/api/articles", CreateValidArticleRequest("Product A"));
        await Client.PostAsJsonAsync("/api/articles", CreateValidArticleRequest("Product B"));

        // Act
        var response = await Client.GetAsync("/api/articles?name=NonExistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().BeEmpty();
        pagedResponse.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetArticles_WithCategoryFilterNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var request = CreateValidArticleRequest("Product");
        request.Category = "Electronics";
        await Client.PostAsJsonAsync("/api/articles", request);

        // Act
        var response = await Client.GetAsync("/api/articles?category=NonExistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().BeEmpty();
        pagedResponse.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetArticles_WithBothFilters_ReturnsMatchingResults()
    {
        // Arrange
        var electronic = CreateValidArticleRequest("Memory Stick");
        electronic.Category = "Electronics";
        await Client.PostAsJsonAsync("/api/articles", electronic);

        var stationary = CreateValidArticleRequest("Memory Foam Pen");
        stationary.Category = "Stationary";
        await Client.PostAsJsonAsync("/api/articles", stationary);

        // Act - Filter by both name and category
        var response = await Client.GetAsync("/api/articles?name=Memory&category=Electronics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().HaveCount(1);
        pagedResponse.Items[0].Name.Should().Be("Memory Stick");
        pagedResponse.Items[0].Category.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetArticles_LastPage_HasNextIsFalse()
    {
        // Arrange - Create exactly 10 articles
        for (int i = 1; i <= 10; i++)
        {
            await Client.PostAsJsonAsync("/api/articles", CreateValidArticleRequest($"Product {i}"));
        }

        // Act - Get page 2 with 5 items (last page)
        var response = await Client.GetAsync("/api/articles?page=2&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Page.Should().Be(2);
        pagedResponse.HasNext.Should().BeFalse();
        pagedResponse.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public async Task GetArticles_PageBeyondTotal_ReturnsEmptyPage()
    {
        // Arrange
        await Client.PostAsJsonAsync("/api/articles", CreateValidArticleRequest("Product 1"));

        // Act - Request page 100
        var response = await Client.GetAsync("/api/articles?page=100");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().BeEmpty();
        pagedResponse.Page.Should().Be(100);
        pagedResponse.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetArticles_WithNegativePageSize_ClampsToMinimum()
    {
        // Arrange
        await Client.PostAsJsonAsync("/api/articles", CreateValidArticleRequest("Product"));

        // Act - Request negative page size (should clamp to 1)
        var response = await Client.GetAsync("/api/articles?pageSize=-5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ArticleResponse>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.PageSize.Should().BeGreaterThan(0);
    }
}
