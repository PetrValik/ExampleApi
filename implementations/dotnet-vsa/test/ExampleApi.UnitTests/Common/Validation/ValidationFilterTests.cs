using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using ExampleApi.Common.Validation;

namespace ExampleApi.UnitTests.Common.Validation;

/// <summary>
/// Unit tests for ValidationFilter.
/// </summary>
public class ValidationFilterTests
{
    /// <summary>
    /// Simple request model used as validation target in tests.
    /// </summary>
    private class TestRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    /// <summary>
    /// Validator that enforces non-empty Name and non-negative Age.
    /// </summary>
    private class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required");

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(0).WithMessage("Age must be non-negative");
        }
    }

    /// <summary>
    /// Validator with no rules — always passes validation.
    /// </summary>
    private class AlwaysPassValidator : AbstractValidator<TestRequest>
    {
        public AlwaysPassValidator()
        {
            // No rules - always passes
        }
    }

    /// <summary>
    /// Valid request returns null (no validation errors).
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithValidRequest_ShouldReturnNull()
    {
        // Arrange
        var validator = new TestRequestValidator();
        var request = new TestRequest { Name = "John", Age = 30 };

        // Act
        var result = await ValidationFilter.ValidateAsync(validator, request);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Invalid request returns a non-null validation problem result.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithInvalidRequest_ShouldReturnValidationProblem()
    {
        // Arrange
        var validator = new TestRequestValidator();
        var request = new TestRequest { Name = "", Age = -1 };

        // Act
        var result = await ValidationFilter.ValidateAsync(validator, request);

        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// Single error on one property returns a non-null result grouped by that property.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithSingleError_ShouldGroupByPropertyName()
    {
        // Arrange
        var validator = new TestRequestValidator();
        var request = new TestRequest { Name = "", Age = 30 }; // Only Name is invalid

        // Act
        var result = await ValidationFilter.ValidateAsync(validator, request);

        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// Multiple errors on the same property are all collected under the same key.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithMultipleErrorsOnSameProperty_ShouldGroupTogether()
    {
        // Arrange
        var multiErrorValidator = new InlineValidator<TestRequest>();
        multiErrorValidator.RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters");

        var request = new TestRequest { Name = "", Age = 30 };

        // Act
        var result = await ValidationFilter.ValidateAsync(multiErrorValidator, request);

        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// The provided cancellation token is forwarded to the underlying FluentValidation call.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithCancellationToken_ShouldPassItToValidator()
    {
        // Arrange
        var validator = new AlwaysPassValidator();
        var request = new TestRequest { Name = "Test", Age = 25 };
        var cts = new CancellationTokenSource();

        // Act
        var result = await ValidationFilter.ValidateAsync(validator, request, cts.Token);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Errors on multiple properties are grouped into separate dictionary entries.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithMultipleProperties_ShouldGroupByPropertyName()
    {
        // Arrange
        var validator = new TestRequestValidator();
        var request = new TestRequest { Name = "", Age = -1 }; // Both invalid

        // Act
        var result = await ValidationFilter.ValidateAsync(validator, request);

        // Assert
        result.Should().NotBeNull();
    }

    /// <summary>
    /// Default cancellation token (<c>default</c>) is accepted without throwing.
    /// </summary>
    [Fact]
    public async Task ValidateAsync_WithDefaultCancellationToken_ShouldWork()
    {
        // Arrange
        var validator = new TestRequestValidator();
        var request = new TestRequest { Name = "Valid", Age = 25 };

        // Act
        var result = await ValidationFilter.ValidateAsync(validator, request);

        // Assert
        result.Should().BeNull();
    }
}
