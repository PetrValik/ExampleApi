using FluentAssertions;
using ExampleApi.Common.Pagination;

namespace ExampleApi.UnitTests.Common.Pagination;

/// <summary>
/// Unit tests for PagedResponse.
/// </summary>
public class PagedResponseTests
{
    /// <summary>
    /// When total count divides evenly by page size, total pages equals the exact quotient.
    /// </summary>
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

    /// <summary>
    /// When total count does not divide evenly, total pages rounds up to the next integer.
    /// </summary>
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

    /// <summary>
    /// Zero total count results in zero total pages.
    /// </summary>
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

    /// <summary>
    /// The first page has no previous page.
    /// </summary>
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

    /// <summary>
    /// Any page after the first has a previous page.
    /// </summary>
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

    /// <summary>
    /// The last page has no next page.
    /// </summary>
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

    /// <summary>
    /// Any page before the last has a next page.
    /// </summary>
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

    /// <summary>
    /// When the only page is both the first and the last, <c>HasNext</c> is false.
    /// </summary>
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
