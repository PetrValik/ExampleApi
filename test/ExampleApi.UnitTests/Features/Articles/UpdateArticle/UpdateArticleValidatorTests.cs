using FluentAssertions;
using ExampleApi.Features.Articles.Shared.DTOs;
using ExampleApi.Features.Articles.UpdateArticle;

namespace ExampleApi.UnitTests.Features.Articles.UpdateArticle;

/// <summary>
/// Unit tests for UpdateArticleValidator.
/// </summary>
public class UpdateArticleValidatorTests
{
    private readonly UpdateArticleValidator _validator;

    public UpdateArticleValidatorTests()
    {
        _validator = new UpdateArticleValidator();
    }

    /// <summary>
    /// Returns a fully valid <see cref="UpdateArticleRequest"/>; individual tests mutate one property to probe a specific rule.
    /// </summary>
    private static UpdateArticleRequest ValidRequest() => new()
    {
        Name = "Test Product",
        Description = "Test Description",
        Price = 9.99m,
        Currency = "USD",
        RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
    };

    /// <summary>
    /// Valid request passes validation without errors.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidRequest_ShouldPass()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Zero price with no currency passes validation (currency not required for free articles).
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithPriceZeroAndNoCurrency_ShouldPass()
    {
        // Arrange
        var request = ValidRequest();
        request.Price = 0;
        request.Currency = null;

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
        var request = ValidRequest();
        request.Name = "";

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
        var request = ValidRequest();
        request.Name = new string('A', 65); // 65 characters

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
        var request = ValidRequest();
        request.Description = "";

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
        var request = ValidRequest();
        request.Description = new string('A', 2049); // 2049 characters

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage.Contains("2048"));
    }

    /// <summary>
    /// Category longer than 64 characters triggers a validation error referencing the maximum length.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithCategoryTooLong_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.Category = new string('A', 65); // 65 characters

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Category" && e.ErrorMessage.Contains("64"));
    }

    /// <summary>
    /// Null Category passes validation as the field is optional.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithNullCategory_ShouldPass()
    {
        // Arrange
        var request = ValidRequest();
        request.Category = null;

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Negative Price triggers a validation error on the Price property.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithNegativePrice_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.Price = -1m;

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    /// <summary>
    /// Positive Price without Currency triggers a validation error on the Currency property.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithPriceGreaterThanZeroAndNoCurrency_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.Price = 10.00m;
        request.Currency = null;

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
        var request = ValidRequest();
        request.Currency = "US"; // Only 2 characters instead of 3

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Currency" && e.ErrorMessage.Contains('3'));
    }

    /// <summary>
    /// Unknown 3-character Currency code triggers a validation error indicating it is not supported.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithUnsupportedCurrencyCode_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.Currency = "BBB"; // Unsupported currency code

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Currency" && e.ErrorMessage.Contains("supported"));
    }

    /// <summary>
    /// Null RowVersion triggers a validation error as the token is required for optimistic concurrency.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithMissingRowVersion_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.RowVersion = null;

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RowVersion");
    }

    /// <summary>
    /// Empty RowVersion byte array triggers a validation error as an empty token is unusable for concurrency control.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithEmptyRowVersion_ShouldFail()
    {
        // Arrange
        var request = ValidRequest();
        request.RowVersion = [];

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RowVersion");
    }
}
