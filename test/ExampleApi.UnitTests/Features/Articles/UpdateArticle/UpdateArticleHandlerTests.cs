using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ExampleApi.Common.Exceptions;
using ExampleApi.Features.Articles.Shared.Models;
using ExampleApi.Features.Articles.UpdateArticle;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.UnitTests.Features.Articles.UpdateArticle;

/// <summary>
/// Unit tests for UpdateArticleHandler.
/// Concurrency conflict scenarios require SQLite and are covered by integration tests.
/// </summary>
public class UpdateArticleHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly UpdateArticleHandler _handler;

    public UpdateArticleHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new UpdateArticleHandler(_dbContext);
    }

    /// <summary>
    /// Valid update request overwrites all fields and persists the changes to the database.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithExistingArticle_ShouldUpdateAllFields()
    {
        // Arrange
        var article = new Article
        {
            Name = "Original Name",
            Description = "Original Description",
            Category = "Original Category",
            Price = 10.00m,
            Currency = "USD"
        };

        _dbContext.Articles.Add(article);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateArticleRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Category = "Updated Category",
            Price = 25.50m,
            Currency = "EUR",
            RowVersion = article.RowVersion
        };

        // Act
        var response = await _handler.HandleAsync(article.ArticleId, request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.ArticleId.Should().Be(article.ArticleId);
        response.Name.Should().Be("Updated Name");
        response.Description.Should().Be("Updated Description");
        response.Category.Should().Be("Updated Category");
        response.Price.Should().Be(25.50m);
        response.Currency.Should().Be("EUR");

        var articleInDb = await _dbContext.Articles.FindAsync(article.ArticleId);
        articleInDb!.Name.Should().Be("Updated Name");
        articleInDb.Description.Should().Be("Updated Description");
    }

    /// <summary>
    /// Null Category in the request clears the existing category value.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithNullCategory_ShouldClearCategory()
    {
        // Arrange
        var article = new Article
        {
            Name = "Test Product",
            Description = "Test Description",
            Category = "Electronics",
            Price = 50.00m,
            Currency = "CZK"
        };

        _dbContext.Articles.Add(article);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateArticleRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Category = null,
            Price = 50.00m,
            Currency = "CZK",
            RowVersion = article.RowVersion
        };

        // Act
        var response = await _handler.HandleAsync(article.ArticleId, request, CancellationToken.None);

        // Assert
        response.Category.Should().BeNull();
    }

    /// <summary>
    /// Zero Price with null Currency is stored correctly (free articles do not require a currency code).
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithZeroPrice_ShouldClearCurrency()
    {
        // Arrange
        var article = new Article
        {
            Name = "Paid Item",
            Description = "Was paid, now free",
            Price = 19.99m,
            Currency = "NOK"
        };

        _dbContext.Articles.Add(article);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateArticleRequest
        {
            Name = "Paid Item",
            Description = "Was paid, now free",
            Price = 0,
            Currency = null,
            RowVersion = article.RowVersion
        };

        // Act
        var response = await _handler.HandleAsync(article.ArticleId, request, CancellationToken.None);

        // Assert
        response.Price.Should().Be(0);
        response.Currency.Should().BeNull();
    }

    /// <summary>
    /// Non-existing article ID throws <see cref="NotFoundException"/> with the ID in the message.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithNonExistingArticle_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistingId = 999;
        var request = new UpdateArticleRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 0,
            RowVersion = 12345678u
        };

        // Act
        var act = async () => await _handler.HandleAsync(nonExistingId, request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Article with ID {nonExistingId} was not found.");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
