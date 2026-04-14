using FluentAssertions;
using ExampleApi.Features.Articles.Shared.Mappings;
using ExampleApi.Features.Articles.Shared.Models;

namespace ExampleApi.UnitTests.Features.Articles.Shared.Mappings;

/// <summary>
/// Unit tests for ArticleMappingExtensions.
/// </summary>
public class ArticleMappingExtensionsTests
{
    [Fact]
    public void ToResponse_WithFullyPopulatedArticle_ShouldMapAllProperties()
    {
        // Arrange
        var article = new Article
        {
            ArticleId = 123,
            Name = "Test Product",
            Description = "Test Description",
            Category = "Electronics",
            Price = 99.99m,
            Currency = "USD",
            RowVersion = 0x01020304u
        };

        // Act
        var response = article.ToResponse();

        // Assert
        response.Should().NotBeNull();
        response.ArticleId.Should().Be(123);
        response.Name.Should().Be("Test Product");
        response.Description.Should().Be("Test Description");
        response.Category.Should().Be("Electronics");
        response.Price.Should().Be(99.99m);
        response.Currency.Should().Be("USD");
        response.RowVersion.Should().Be(0x01020304u);
    }

    [Fact]
    public void ToResponse_WithNullableFieldsNull_ShouldMapCorrectly()
    {
        // Arrange
        var article = new Article
        {
            ArticleId = 1,
            Name = "Free Item",
            Description = "Free product",
            Category = null,
            Price = 0m,
            Currency = null,
            RowVersion = 0u
        };

        // Act
        var response = article.ToResponse();

        // Assert
        response.Should().NotBeNull();
        response.ArticleId.Should().Be(1);
        response.Name.Should().Be("Free Item");
        response.Description.Should().Be("Free product");
        response.Category.Should().BeNull();
        response.Price.Should().Be(0m);
        response.Currency.Should().BeNull();
        response.RowVersion.Should().Be(0u);
    }

    [Fact]
    public void ToResponse_WithZeroPrice_ShouldMapCorrectly()
    {
        // Arrange
        var article = new Article
        {
            ArticleId = 42,
            Name = "Free Sample",
            Description = "This is free",
            Price = 0m,
            Currency = null
        };

        // Act
        var response = article.ToResponse();

        // Assert
        response.ArticleId.Should().Be(42);
        response.Price.Should().Be(0m);
        response.Currency.Should().BeNull();
    }

    [Fact]
    public void ToResponse_WithDecimalPrice_ShouldPreservePrecision()
    {
        // Arrange
        var article = new Article
        {
            ArticleId = 1,
            Name = "Precision Test",
            Description = "Testing decimal precision",
            Price = 12.345m,
            Currency = "EUR"
        };

        // Act
        var response = article.ToResponse();

        // Assert
        response.Price.Should().Be(12.345m);
    }

    [Fact]
    public void ToResponse_WithLargePriceValue_ShouldMapCorrectly()
    {
        // Arrange
        var article = new Article
        {
            ArticleId = 999,
            Name = "Expensive Item",
            Description = "Very expensive",
            Price = 999999.99m,
            Currency = "USD"
        };

        // Act
        var response = article.ToResponse();

        // Assert
        response.Price.Should().Be(999999.99m);
        response.Currency.Should().Be("USD");
    }

    [Fact]
    public void ToResponse_WithEmptyCategory_ShouldMapAsNull()
    {
        // Arrange
        var article = new Article
        {
            ArticleId = 5,
            Name = "No Category",
            Description = "Product without category",
            Category = null,
            Price = 10.00m,
            Currency = "GBP"
        };

        // Act
        var response = article.ToResponse();

        // Assert
        response.Category.Should().BeNull();
    }

    [Fact]
    public void ToResponse_WithNonZeroRowVersion_ShouldMapValue()
    {
        // Arrange
        const uint rowVersion = 0x01020304u;
        var article = new Article
        {
            ArticleId = 10,
            Name = "Versioned",
            Description = "Has row version",
            Price = 50.00m,
            Currency = "SEK",
            RowVersion = rowVersion
        };

        // Act
        var response = article.ToResponse();

        // Assert
        response.RowVersion.Should().Be(rowVersion);
    }

    [Fact]
    public void ToResponse_MultipleArticles_ShouldMapIndependently()
    {
        // Arrange
        var article1 = new Article
        {
            ArticleId = 1,
            Name = "Product 1",
            Description = "First product",
            Price = 10.00m,
            Currency = "USD"
        };

        var article2 = new Article
        {
            ArticleId = 2,
            Name = "Product 2",
            Description = "Second product",
            Price = 20.00m,
            Currency = "EUR"
        };

        // Act
        var response1 = article1.ToResponse();
        var response2 = article2.ToResponse();

        // Assert
        response1.ArticleId.Should().Be(1);
        response1.Name.Should().Be("Product 1");
        response1.Price.Should().Be(10.00m);
        response1.Currency.Should().Be("USD");

        response2.ArticleId.Should().Be(2);
        response2.Name.Should().Be("Product 2");
        response2.Price.Should().Be(20.00m);
        response2.Currency.Should().Be("EUR");
    }
}
