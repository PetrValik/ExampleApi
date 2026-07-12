namespace ExampleApi.Common.Currency;

/// <summary>
/// The set of ISO 4217 alphabetic currency codes supported by the API.
/// </summary>
public static class CurrencyCodes
{
    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
    {
        // Major
        "USD", "EUR", "JPY", "GBP", "CHF", "CAD", "AUD", "NZD",
        // European
        "SEK", "NOK", "DKK", "CZK", "PLN", "HUF", "RON", "BGN", "ISK",
        // Asian
        "CNY", "HKD", "SGD", "KRW", "INR", "THB", "MYR", "IDR", "PHP", "TWD", "VND",
        // Latin American
        "BRL", "MXN", "ARS", "CLP", "COP", "PEN",
        // Middle Eastern
        "AED", "SAR", "ILS", "QAR", "KWD", "BHD",
        // Other
        "ZAR", "TRY", "RUB", "UAH", "EGP", "NGN", "KES", "MAD", "PKR",
    };

    /// <summary>Returns <c>true</c> when the supplied currency code is supported.</summary>
    public static bool IsSupported(string? currencyCode) =>
        !string.IsNullOrWhiteSpace(currencyCode) && Supported.Contains(currencyCode);

    /// <summary>All supported currency codes.</summary>
    public static IReadOnlyCollection<string> All => Supported;
}
