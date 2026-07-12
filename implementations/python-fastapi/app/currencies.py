"""Supported ISO 4217 currency codes (the 49 pinned by the contract)."""

from __future__ import annotations

SUPPORTED_CURRENCIES: frozenset[str] = frozenset(
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
    """Case-insensitive membership test against the supported set."""
    return bool(code) and code.upper() in SUPPORTED_CURRENCIES
