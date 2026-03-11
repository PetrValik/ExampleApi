using FluentAssertions;
using ExampleApi.Common.Currency;

namespace ExampleApi.UnitTests.Common.Currency;

/// <summary>
/// Unit tests for CurrencyCodes utility class.
/// </summary>
public class CurrencyCodesTests
{
    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("CHF")]
    [InlineData("CAD")]
    [InlineData("AUD")]
    [InlineData("SEK")]
    [InlineData("NOK")]
    [InlineData("DKK")]
    [InlineData("CZK")]
    [InlineData("PLN")]
    public void IsSupported_WithValidCurrency_ShouldReturnTrue(string currencyCode)
    {
        // Act
        var result = CurrencyCodes.IsSupported(currencyCode);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("usd")] // Lowercase
    [InlineData("Usd")] // Mixed case
    [InlineData("UsD")]
    [InlineData("eur")]
    [InlineData("nok")]
    public void IsSupported_IsCaseInsensitive(string currencyCode)
    {
        // Act
        var result = CurrencyCodes.IsSupported(currencyCode);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("XXX")]
    [InlineData("ABC")]
    [InlineData("ZZZ")]
    [InlineData("INVALID")]
    public void IsSupported_WithInvalidCurrency_ShouldReturnFalse(string currencyCode)
    {
        // Act
        var result = CurrencyCodes.IsSupported(currencyCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSupported_WithNull_ShouldReturnFalse()
    {
        // Act
        var result = CurrencyCodes.IsSupported(null);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsSupported_WithEmptyOrWhitespace_ShouldReturnFalse(string currencyCode)
    {
        // Act
        var result = CurrencyCodes.IsSupported(currencyCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetAll_ShouldReturnNonEmptyCollection()
    {
        // Act
        var currencies = CurrencyCodes.GetAll();

        // Assert
        currencies.Should().NotBeNull();
        currencies.Should().NotBeEmpty();
        currencies.Should().Contain("USD");
        currencies.Should().Contain("EUR");
        currencies.Should().Contain("NOK");
    }

    [Fact]
    public void GetAll_ShouldReturnReadOnlyCollection()
    {
        // Act
        var currencies = CurrencyCodes.GetAll();

        // Assert
        currencies.Should().BeAssignableTo<IReadOnlyCollection<string>>();
    }

    [Fact]
    public void GetAll_ShouldReturnSameCollectionOnMultipleCalls()
    {
        // Act
        var currencies1 = CurrencyCodes.GetAll();
        var currencies2 = CurrencyCodes.GetAll();

        // Assert
        currencies1.Should().BeEquivalentTo(currencies2);
    }

    [Theory]
    [InlineData("BRL")] // Brazilian Real
    [InlineData("MXN")] // Mexican Peso
    [InlineData("CNY")] // Chinese Yuan
    [InlineData("INR")] // Indian Rupee
    [InlineData("AED")] // UAE Dirham
    [InlineData("ZAR")] // South African Rand
    public void IsSupported_WithVariousInternationalCurrencies_ShouldReturnTrue(string currencyCode)
    {
        // Act
        var result = CurrencyCodes.IsSupported(currencyCode);

        // Assert
        result.Should().BeTrue();
    }
}
