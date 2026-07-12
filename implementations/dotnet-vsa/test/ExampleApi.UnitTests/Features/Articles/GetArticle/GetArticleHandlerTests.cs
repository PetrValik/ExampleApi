using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ExampleApi.Common.Exceptions;
using ExampleApi.Features.Articles.GetArticle;
using ExampleApi.Features.Articles.Shared.Models;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.UnitTests.Features.Articles.GetArticle;

/// <summary>
/// Unit tests for GetArticleHandler.
/// </summary>
public class GetArticleHandlerTests : IDisposable
{
    /// <summary>
    /// In-memory <see cref="AppDbContext"/> used to seed test data.
    /// </summary>
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// The handler under test.
    /// </summary>
    private readonly GetArticleHandler _handler;

    /// <summary>
    /// Initializes the in-memory database and the handler under test.
    /// </summary>
    public GetArticleHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new GetArticleHandler(_dbContext);
    }

    /// <summary>
    /// Existing article ID returns the correct article with all fields populated.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithExistingArticle_ShouldReturnArticle()
    {
        // Arrange
        var article = new Article
        {
            Name = "Test Product",
            Description = "Test Description",
            Category = "Electronics",
            Price = 99.99m,
            Currency = "USD"
        };

        _dbContext.Articles.Add(article);
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _handler.HandleAsync(article.ArticleId, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.ArticleId.Should().Be(article.ArticleId);
        response.Name.Should().Be(article.Name);
        response.Description.Should().Be(article.Description);
        response.Category.Should().Be(article.Category);
        response.Price.Should().Be(article.Price);
        response.Currency.Should().Be(article.Currency);
    }

    /// <summary>
    /// Non-existing article ID throws <see cref="NotFoundException"/> with the ID in the message.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithNonExistingArticle_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        var act = async () => await _handler.HandleAsync(nonExistingId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Article with ID {nonExistingId} not found.");
    }

    /// <summary>
    /// Disposes the in-memory database context.
    /// </summary>
    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
