using FluentAssertions;
using ExampleApi.Features.Articles.BatchCreateArticles;
using ExampleApi.Features.Articles.Shared.DTOs;

namespace ExampleApi.UnitTests.Features.Articles.BatchCreateArticles;

/// <summary>
/// Unit tests for BatchCreateArticlesValidator.
/// </summary>
public class BatchCreateArticlesValidatorTests
{
    private readonly BatchCreateArticlesValidator _validator;

    public BatchCreateArticlesValidatorTests()
    {
        _validator = new BatchCreateArticlesValidator();
    }

    /// <summary>
    /// Returns a valid <see cref="ArticleRequest"/> for testing.
    /// </summary>
    private static ArticleRequest ValidArticleRequest() => new()
    {
        Name = "Test Product",
        Description = "Test Description",
        Price = 9.99m,
        Currency = "USD"
    };

    /// <summary>
    /// Valid list of articles passes validation without errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidList_ShouldPass()
    {
        // Arrange
        var request = new List<ArticleRequest>
        {
            ValidArticleRequest(),
            new()
            {
                Name = "Second Product",
                Description = "Second Description",
                Price = 19.99m,
                Currency = "EUR"
            }
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Single valid article in list passes validation.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithSingleValidArticle_ShouldPass()
    {
        // Arrange
        var request = new List<ArticleRequest> { ValidArticleRequest() };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Empty list triggers validation error.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithEmptyList_ShouldFail()
    {
        // Arrange
        var request = new List<ArticleRequest>();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("At least one article"));
    }

    /// <summary>
    /// List with one invalid article triggers validation error for that article.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithOneInvalidArticle_ShouldFail()
    {
        // Arrange
        var request = new List<ArticleRequest>
        {
            ValidArticleRequest(),
            new()
            {
                Name = "", // Invalid: empty name
                Description = "Valid Description",
                Price = 9.99m,
                Currency = "USD"
            }
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("[1].Name"));
    }

    /// <summary>
    /// List with multiple invalid articles triggers multiple validation errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithMultipleInvalidArticles_ShouldFail()
    {
        // Arrange
        var request = new List<ArticleRequest>
        {
            new()
            {
                Name = "", // Invalid: empty name
                Description = "Description",
                Price = 9.99m,
                Currency = "USD"
            },
            new()
            {
                Name = "Valid Name",
                Description = "Valid Description",
                Price = 10.00m,
                Currency = null // Invalid: price > 0 but no currency
            }
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("[0].Name"));
        result.Errors.Should().Contain(e => e.PropertyName.Contains("[1].Currency"));
    }

    /// <summary>
    /// Article with price greater than zero requires currency.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithPriceButNoCurrency_ShouldFail()
    {
        // Arrange
        var request = new List<ArticleRequest>
        {
            new()
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 9.99m,
                Currency = null // Invalid: price > 0 but no currency
            }
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Currency"));
    }

    /// <summary>
    /// Free articles (price = 0) do not require currency.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithPriceZeroAndNoCurrency_ShouldPass()
    {
        // Arrange
        var request = new List<ArticleRequest>
        {
            new()
            {
                Name = "Free Product",
                Description = "Free Description",
                Price = 0,
                Currency = null
            }
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Article with invalid currency code triggers validation error.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithInvalidCurrencyCode_ShouldFail()
    {
        // Arrange
        var request = new List<ArticleRequest>
        {
            new()
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 9.99m,
                Currency = "XXX" // Invalid currency code
            }
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Currency"));
    }
}
