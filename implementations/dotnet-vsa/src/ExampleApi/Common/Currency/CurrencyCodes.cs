namespace ExampleApi.Common.Currency;

/// <summary>
/// Provides validation for currency codes supported by the application.
/// </summary>
public static class CurrencyCodes
{
    /// <summary>
    /// Set of supported ISO 4217 alphabetic currency codes.
    /// </summary>
    private static readonly HashSet<string> SupportedCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        // Major currencies
        "USD", "EUR", "JPY", "GBP", "CHF", "CAD", "AUD", "NZD",

        // European currencies
        "SEK", "NOK", "DKK", "CZK", "PLN", "HUF", "RON", "BGN", "ISK",

        // Asian currencies
        "CNY", "HKD", "SGD", "KRW", "INR", "THB", "MYR", "IDR", "PHP", "TWD", "VND",

        // Latin American currencies
        "BRL", "MXN", "ARS", "CLP", "COP", "PEN",

        // Middle Eastern currencies
        "AED", "SAR", "ILS", "QAR", "KWD", "BHD",

        // Other currencies
        "ZAR", "TRY", "RUB", "UAH", "EGP", "NGN", "KES", "MAD", "PKR"
    };

    /// <summary>
    /// Validates whether the specified currency code is supported by the application.
    /// </summary>
    /// <param name="currencyCode">The currency code to validate.</param>
    /// <returns>True if the currency code is supported; otherwise, false.</returns>
    public static bool IsSupported(string? currencyCode)
    {
        return !string.IsNullOrWhiteSpace(currencyCode)
            && SupportedCurrencies.Contains(currencyCode);
    }

    /// <summary>
    /// Gets all supported currency codes.
    /// </summary>
    /// <returns>A read-only collection of supported currency codes.</returns>
    public static IReadOnlyCollection<string> GetAll()
    {
        return SupportedCurrencies;
    }
}
