using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ExampleApi.Common.Exceptions;
using ExampleApi.Features.Articles.DeleteArticle;
using ExampleApi.Features.Articles.Shared.Models;
using ExampleApi.Infrastructure.Database;

namespace ExampleApi.UnitTests.Features.Articles.DeleteArticle;

/// <summary>
/// Unit tests for DeleteArticleHandler.
/// </summary>
public class DeleteArticleHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly DeleteArticleHandler _handler;

    public DeleteArticleHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _handler = new DeleteArticleHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<Article> SeedArticleAsync(string name = "Test Article")
    {
        var article = new Article
        {
            Name = name,
            Description = "Test Description",
            Category = "Electronics",
            Price = 9.99m,
            Currency = "USD"
        };

        _dbContext.Articles.Add(article);
        await _dbContext.SaveChangesAsync();
        return article;
    }

    /// <summary>
    /// Existing article ID removes the article from the database.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithExistingArticle_ShouldDeleteArticle()
    {
        // Arrange
        var article = await SeedArticleAsync();

        // Act
        await _handler.HandleAsync(article.ArticleId, CancellationToken.None);

        // Assert
        var deletedArticle = await _dbContext.Articles.FindAsync(article.ArticleId);
        deletedArticle.Should().BeNull();
    }

    /// <summary>
    /// Only the targeted article is removed; other articles remain untouched.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithExistingArticle_ShouldOnlyDeleteTargetArticle()
    {
        // Arrange
        var articleToDelete = await SeedArticleAsync("To Delete");
        var articleToKeep = await SeedArticleAsync("To Keep");

        // Act
        await _handler.HandleAsync(articleToDelete.ArticleId, CancellationToken.None);

        // Assert
        var remaining = await _dbContext.Articles.FindAsync(articleToKeep.ArticleId);
        remaining.Should().NotBeNull();
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
            .WithMessage($"Article with ID {nonExistingId} was not found.");
    }

    /// <summary>
    /// Cancelled token throws <see cref="OperationCanceledException"/> before any database operation.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var article = await SeedArticleAsync();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = async () => await _handler.HandleAsync(article.ArticleId, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}