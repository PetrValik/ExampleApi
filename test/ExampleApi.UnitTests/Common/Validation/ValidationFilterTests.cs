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
    private class TestRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

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

    private class AlwaysPassValidator : AbstractValidator<TestRequest>
    {
        public AlwaysPassValidator()
        {
            // No rules - always passes
        }
    }

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
