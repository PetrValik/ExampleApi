using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ExampleApi.Features.Articles.CreateArticle;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.Features.Articles.Shared.Models;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.UnitTests.Features.Articles.CreateArticle;

/// <summary>
/// Unit tests for CreateArticleHandler.
/// </summary>
public class CreateArticleHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly CreateArticleHandler _handler;

    public CreateArticleHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new CreateArticleHandler(_dbContext);
    }

    /// <summary>
    /// Valid request creates an article with a generated ID and all fields persisted correctly.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithValidRequest_ShouldCreateArticle()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Category = "Electronics",
            Price = 99.99m,
            Currency = "USD"
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.ArticleId.Should().BeGreaterThan(0);
        response.Name.Should().Be(request.Name);
        response.Description.Should().Be(request.Description);
        response.Category.Should().Be(request.Category);
        response.Price.Should().Be(request.Price);
        response.Currency.Should().Be(request.Currency);

        var articleInDb = await _dbContext.Articles.FindAsync(response.ArticleId);
        articleInDb.Should().NotBeNull();
        articleInDb!.Name.Should().Be(request.Name);
    }

    /// <summary>
    /// Null Category is stored correctly as the field is optional.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithNullCategory_ShouldCreateArticle()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Category = null,
            Price = 50.00m,
            Currency = "EUR"
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Category.Should().BeNull();
    }

    /// <summary>
    /// Zero Price with null Currency is stored correctly (free articles do not require a currency code).
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithZeroPrice_ShouldCreateArticle()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Free Item",
            Description = "Free promotional item",
            Price = 0,
            Currency = null
        };

        // Act
        var response = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Price.Should().Be(0);
        response.Currency.Should().BeNull();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
