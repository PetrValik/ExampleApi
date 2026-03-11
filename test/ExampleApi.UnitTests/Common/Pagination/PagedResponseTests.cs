using FluentAssertions;
using ExampleApi.Common.Pagination;

namespace ExampleApi.UnitTests.Common.Pagination;

/// <summary>
/// Unit tests for PagedResponse.
/// </summary>
public class PagedResponseTests
{
    [Fact]
    public void TotalPages_WithExactDivision_ShouldReturnCorrectValue()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 100
        };

        // Act & Assert
        response.TotalPages.Should().Be(10);
    }

    [Fact]
    public void TotalPages_WithRemainder_ShouldRoundUp()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 95
        };

        // Act & Assert
        response.TotalPages.Should().Be(10); // 95 / 10 = 9.5 → rounds up to 10
    }

    [Fact]
    public void TotalPages_WithZeroItems_ShouldReturnZero()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 0
        };

        // Act & Assert
        response.TotalPages.Should().Be(0);
    }

    [Fact]
    public void HasPrevious_OnFirstPage_ShouldBeFalse()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 100
        };

        // Act & Assert
        response.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void HasPrevious_OnSecondOrLaterPage_ShouldBeTrue()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Items = [],
            Page = 2,
            PageSize = 10,
            TotalCount = 100
        };

        // Act & Assert
        response.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void HasNext_OnLastPage_ShouldBeFalse()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Items = [],
            Page = 10,
            PageSize = 10,
            TotalCount = 100
        };

        // Act & Assert
        response.HasNext.Should().BeFalse();
    }

    [Fact]
    public void HasNext_BeforeLastPage_ShouldBeTrue()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 100
        };

        // Act & Assert
        response.HasNext.Should().BeTrue();
    }

    [Fact]
    public void HasNext_OnSinglePage_ShouldBeFalse()
    {
        // Arrange
        var response = new PagedResponse<string>
        {
            Items = [],
            Page = 1,
            PageSize = 10,
            TotalCount = 5
        };

        // Act & Assert
        response.HasNext.Should().BeFalse();
        response.TotalPages.Should().Be(1);
    }
}
