"""Explicit business-rule validation producing field-keyed error maps.

Mirrors the reference (dotnet-vsa) validators exactly, so every failing rule
returns 400 problem+json with an ``errors`` object keyed by the offending field.
"""

from __future__ import annotations

from app.currencies import is_supported
from app.schemas import ArticleRequest, UpdateArticleRequest

MAX_PRICE = 9_999_999_999_999_999.99


def _validate_common(req: ArticleRequest, errors: dict[str, list[str]]) -> None:
    # name: required, 1..64
    name = req.name or ""
    if name == "":
        errors.setdefault("name", []).append("Name is required.")
    elif len(name) > 64:
        errors.setdefault("name", []).append("Name must not exceed 64 characters.")

    # description: required, 1..2048
    description = req.description or ""
    if description == "":
        errors.setdefault("description", []).append("Description is required.")
    elif len(description) > 2048:
        errors.setdefault("description", []).append(
            "Description must not exceed 2048 characters."
        )

    # category: optional, <= 64
    if req.category is not None and len(req.category) > 64:
        errors.setdefault("category", []).append("Category must not exceed 64 characters.")

    # price: required, 0 <= price <= MAX_PRICE
    if req.price is None:
        errors.setdefault("price", []).append("Price is required.")
    else:
        if req.price < 0:
            errors.setdefault("price", []).append(
                "Price must be greater than or equal to 0."
            )
        if req.price > MAX_PRICE:
            errors.setdefault("price", []).append(
                "Price must not exceed 9,999,999,999,999,999.99."
            )

    # currency: required & validated only when price > 0
    if req.price is not None and req.price > 0:
        currency = req.currency
        if not currency:
            errors.setdefault("currency", []).append(
                "Currency is required when price is greater than 0."
            )
        elif len(currency) != 3:
            errors.setdefault("currency", []).append(
                "Currency must be a valid ISO 4217 code (3 characters)."
            )
        elif not is_supported(currency):
            errors.setdefault("currency", []).append(
                "Currency must be a supported currency code."
            )


def validate_create(req: ArticleRequest) -> dict[str, list[str]]:
    errors: dict[str, list[str]] = {}
    _validate_common(req, errors)
    return errors


def validate_update(req: UpdateArticleRequest) -> dict[str, list[str]]:
    errors: dict[str, list[str]] = {}
    _validate_common(req, errors)
    # row_version: required and >= 1
    if req.row_version is None or req.row_version < 1:
        errors.setdefault("row_version", []).append(
            "Row version is required and must be a valid non-zero value."
        )
    return errors
