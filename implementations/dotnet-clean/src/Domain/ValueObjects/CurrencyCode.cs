namespace ExampleApi.Domain.ValueObjects;

/// <summary>
/// Domain knowledge of the currency codes the shop is allowed to price articles in.
/// The supported set is a business rule, so it lives in the Domain layer; the
/// Application layer's validators consult it to reject unsupported codes with a 400.
/// </summary>
public static class CurrencyCode
{
    /// <summary>
    /// The 49 supported ISO 4217 alphabetic currency codes.
    /// </summary>
    private static readonly HashSet<string> Supported = new(StringComparer.OrdinalIgnoreCase)
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
    /// Returns <c>true</c> when the supplied code is a supported ISO 4217 currency.
    /// </summary>
    /// <param name="code">The candidate currency code (case-insensitive).</param>
    public static bool IsSupported(string? code) =>
        !string.IsNullOrWhiteSpace(code) && Supported.Contains(code);

    /// <summary>
    /// All supported currency codes.
    /// </summary>
    public static IReadOnlyCollection<string> All => Supported;
}
