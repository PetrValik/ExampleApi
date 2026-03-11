using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ExampleApi.Features.Articles.GetArticles;
using ExampleApi.Features.Articles.Shared.Models;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.UnitTests.Features.Articles.GetArticles;

/// <summary>
/// Unit tests for GetArticlesHandler.
/// </summary>
public class GetArticlesHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly GetArticlesHandler _handler;

    public GetArticlesHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new GetArticlesHandler(_dbContext);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var articles = new[]
        {
            new Article
            {
                Name = "Branded Memory Stick",
                Description = "Branded 16 GB memory stick",
                Category = "USB flash drive",
                Price = 17.89m,
                Currency = "NOK"
            },
            new Article
            {
                Name = "Branded Drinking Mug",
                Description = "Porcelain drinking cup with your logo on it.",
                Category = "Mug",
                Price = 0,
                Currency = null
            },
            new Article
            {
                Name = "Branded Pen",
                Description = "High quality pen with logo",
                Category = "Stationery",
                Price = 5.99m,
                Currency = "USD"
            }
        };

        _dbContext.Articles.AddRange(articles);
        _dbContext.SaveChanges();
    }

    /// <summary>
    /// Request without filters returns all seeded articles.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithNoFilters_ShouldReturnAllArticles()
    {
        // Arrange
        var request = new GetArticlesRequest();

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Items.Should().HaveCount(3);
        response.TotalCount.Should().Be(3);
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
    }

    /// <summary>
    /// Name filter returns all articles whose name contains the search term (partial match).
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithNameFilter_ShouldReturnMatchingArticles()
    {
        // Arrange
        var request = new GetArticlesRequest { Name = "Branded" };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Items.Should().HaveCount(3);
        response.Items.Should().AllSatisfy(article => 
            article.Name.Should().Contain("Branded", 
                because: "all test articles contain 'Branded' in the name"));
        response.TotalCount.Should().Be(3);
    }

    /// <summary>
    /// Name filter is case-insensitive; lowercase term matches articles regardless of casing.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithNameFilterCaseInsensitive_ShouldReturnMatchingArticles()
    {
        // Arrange
        var request = new GetArticlesRequest { Name = "memory" };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Items.Should().HaveCount(1);
        response.Items.First().Name.Should().Be("Branded Memory Stick");
        response.TotalCount.Should().Be(1);
    }

    /// <summary>
    /// Category filter returns only articles with an exact category match.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithCategoryFilter_ShouldReturnExactMatch()
    {
        // Arrange
        var request = new GetArticlesRequest { Category = "USB flash drive" };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Items.Should().HaveCount(1);
        response.Items.First().Category.Should().Be("USB flash drive");
        response.TotalCount.Should().Be(1);
    }

    /// <summary>
    /// Combining Name and Category filters narrows results to articles matching both criteria.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithBothFilters_ShouldReturnMatchingArticles()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Name = "Branded",
            Category = "Mug"
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Items.Should().HaveCount(1);
        response.Items.First().Name.Should().Be("Branded Drinking Mug");
        response.TotalCount.Should().Be(1);
    }

    /// <summary>
    /// Filter matching no articles returns an empty list (not 404).
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var request = new GetArticlesRequest { Name = "NonExistent" };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    /// <summary>
    /// Pagination returns correct page with correct number of items.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Page = 2,
            PageSize = 1
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Items.Should().HaveCount(1);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(1);
        response.TotalCount.Should().Be(3);
        response.TotalPages.Should().Be(3);
        response.HasPrevious.Should().BeTrue();
        response.HasNext.Should().BeTrue();
    }

    /// <summary>
    /// Page size exceeding maximum should be clamped to 100.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithExcessivePageSize_ShouldClampToMaximum()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Page = 1,
            PageSize = 200
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.PageSize.Should().Be(100);
    }

    /// <summary>
    /// Invalid page number (0 or negative) should default to 1.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithInvalidPageNumber_ShouldDefaultToPageOne()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Page = 0,
            PageSize = 10
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Page.Should().Be(1);
    }

    /// <summary>
    /// Negative page number should default to 1.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithNegativePageNumber_ShouldDefaultToPageOne()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Page = -5,
            PageSize = 10
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Page.Should().Be(1);
    }

    /// <summary>
    /// Page size of 0 should default to minimum of 1.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithZeroPageSize_ShouldClampToMinimum()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Page = 1,
            PageSize = 0
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.PageSize.Should().Be(1);
    }

    /// <summary>
    /// Negative page size should clamp to minimum of 1.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithNegativePageSize_ShouldClampToMinimum()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Page = 1,
            PageSize = -10
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.PageSize.Should().Be(1);
    }

    /// <summary>
    /// First page should have no previous page.
    /// </summary>
    [Fact]
    public async Task HandleAsync_OnFirstPage_HasPreviousShouldBeFalse()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Page = 1,
            PageSize = 2
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.HasPrevious.Should().BeFalse();
    }

    /// <summary>
    /// Last page should have no next page.
    /// </summary>
    [Fact]
    public async Task HandleAsync_OnLastPage_HasNextShouldBeFalse()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Page = 3,
            PageSize = 1
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Items.Should().HaveCount(1);
        response.HasNext.Should().BeFalse();
        response.HasPrevious.Should().BeTrue();
    }

    /// <summary>
    /// Page beyond total pages should return empty results.
    /// </summary>
    [Fact]
    public async Task HandleAsync_PageBeyondTotal_ShouldReturnEmptyResults()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Page = 100,
            PageSize = 10
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(3);
        response.Page.Should().Be(100);
    }

    /// <summary>
    /// Null page and page size should use defaults.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithNullPageAndPageSize_ShouldUseDefaults()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Page = null,
            PageSize = null
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.Items.Should().HaveCount(3);
    }

    /// <summary>
    /// White space name filter should be ignored (return all).
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithWhitespaceNameFilter_ShouldReturnAll()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Name = "   ",
            Page = 1,
            PageSize = 10
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Items.Should().HaveCount(3);
    }

    /// <summary>
    /// White space category filter should be ignored (return all).
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithWhitespaceCategoryFilter_ShouldReturnAll()
    {
        // Arrange
        var request = new GetArticlesRequest 
        { 
            Category = "   ",
            Page = 1,
            PageSize = 10
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Items.Should().HaveCount(3);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
