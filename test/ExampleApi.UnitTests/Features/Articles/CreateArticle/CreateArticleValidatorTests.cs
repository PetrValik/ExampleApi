using FluentAssertions;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.Features.Articles.Shared.Validators;

namespace ExampleApi.UnitTests.Features.Articles.CreateArticle;

/// <summary>
/// Unit tests for CreateArticleValidator.
/// </summary>
public class CreateArticleValidatorTests
{
    private readonly ArticleRequestValidator _validator;

    public CreateArticleValidatorTests()
    {
        _validator = new ArticleRequestValidator();
    }

    /// <summary>
    /// Valid request passes validation without errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidRequest_ShouldPass()
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
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Empty Name triggers a validation error on the Name property.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithEmptyName_ShouldFail()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "",
            Description = "Test Description",
            Price = 10.00m,
            Currency = "USD"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    /// <summary>
    /// Name longer than 64 characters triggers a validation error referencing the maximum length.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithNameTooLong_ShouldFail()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = new string('A', 65), // 65 characters
            Description = "Test Description",
            Price = 10.00m,
            Currency = "USD"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("64"));
    }

    /// <summary>
    /// Empty Description triggers a validation error on the Description property.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithEmptyDescription_ShouldFail()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Test Product",
            Description = "",
            Price = 10.00m,
            Currency = "USD"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    /// <summary>
    /// Description longer than 2048 characters triggers a validation error referencing the maximum length.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithDescriptionTooLong_ShouldFail()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Test Product",
            Description = new string('A', 2049), // 2049 characters
            Price = 10.00m,
            Currency = "USD"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage.Contains("2048"));
    }

    /// <summary>
    /// Negative Price triggers a validation error on the Price property.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithNegativePrice_ShouldFail()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = -10.00m
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    /// <summary>
    /// Zero price with no currency passes validation (currency not required for free articles).
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithPriceZeroAndNoCurrency_ShouldPass()
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
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Positive Price without Currency triggers a validation error on the Currency property.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithPriceGreaterThanZeroAndNoCurrency_ShouldFail()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.00m,
            Currency = null
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Currency");
    }

    /// <summary>
    /// Currency code not exactly 3 characters triggers a validation error referencing the required length.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithInvalidCurrencyLength_ShouldFail()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.00m,
            Currency = "US" // Only 2 characters instead of 3
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Currency" && e.ErrorMessage.Contains('3'));
    }

    /// <summary>
    /// Category longer than 64 characters triggers a validation error referencing the maximum length.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithCategoryTooLong_ShouldFail()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Category = new string('A', 65), // 65 characters
            Price = 10.00m,
            Currency = "USD"
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Category" && e.ErrorMessage.Contains("64"));
    }

    /// <summary>
    /// Unknown 3-character Currency code triggers a validation error indicating it is not supported.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithInvalidCurrencyCode_ShouldFail()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.00m,
            Currency = "BBB" // Unsupported currency code
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Currency" && e.ErrorMessage.Contains("supported"));
    }

    /// <summary>
    /// Valid ISO 4217 currency code passes validation.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidCurrencyCode_ShouldPass()
    {
        // Arrange
        var request = new ArticleRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 10.00m,
            Currency = "NOK" // Valid currency code
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
