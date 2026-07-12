"""Supported ISO-4217 currency codes (the 49 defined by the shared contract)."""

# Exact set from contract/openapi.yaml (CurrencyCode enum) — 49 codes.
SUPPORTED_CURRENCIES = frozenset(
    {
        # Major
        "USD", "EUR", "JPY", "GBP", "CHF", "CAD", "AUD", "NZD",
        # European
        "SEK", "NOK", "DKK", "CZK", "PLN", "HUF", "RON", "BGN", "ISK",
        # Asian
        "CNY", "HKD", "SGD", "KRW", "INR", "THB", "MYR", "IDR", "PHP", "TWD", "VND",
        # Latin American
        "BRL", "MXN", "ARS", "CLP", "COP", "PEN",
        # Middle Eastern
        "AED", "SAR", "ILS", "QAR", "KWD", "BHD",
        # Other
        "ZAR", "TRY", "RUB", "UAH", "EGP", "NGN", "KES", "MAD", "PKR",
    }
)


def is_supported(code):
    """True when ``code`` is a supported 3-letter code (case-insensitive)."""
    return isinstance(code, str) and code.upper() in SUPPORTED_CURRENCIES
