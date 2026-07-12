"""Supported ISO 4217 currency codes (the 49 codes pinned by the contract)."""

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


def is_supported(code: str | None) -> bool:
    """Return True when ``code`` is a supported ISO 4217 alphabetic code."""
    return bool(code) and code.upper() in SUPPORTED_CURRENCIES
