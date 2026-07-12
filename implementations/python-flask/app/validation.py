"""Manual request-body validation.

Each validator mutates an ``errors`` dict (field name -> list[str]) and returns a
cleaned data dict. Rules mirror the contract exactly:

* name         required, 1-64 chars
* description  required, 1-2048 chars
* category     optional; if present, <= 64 chars
* price        number, >= 0 and <= 9999999999999999.99
* currency     required + supported ISO-4217 ONLY when price > 0; ignored at price 0
* row_version  (update only) required, integer >= 1
"""

from .currency import is_supported

_MISSING = object()

MAX_PRICE = 9999999999999999.99


def _is_number(value):
    # bool is a subclass of int — reject it explicitly.
    return isinstance(value, (int, float)) and not isinstance(value, bool)


def _add(errors, field, message):
    errors.setdefault(field, []).append(message)


def _validate_common(body, errors):
    data = {}

    # name -------------------------------------------------------------------
    name = body.get("name")
    if not isinstance(name, str) or name.strip() == "":
        _add(errors, "name", "Name is required.")
    elif len(name) > 64:
        _add(errors, "name", "Name must not exceed 64 characters.")
    else:
        data["name"] = name

    # description ------------------------------------------------------------
    description = body.get("description")
    if not isinstance(description, str) or description == "":
        _add(errors, "description", "Description is required.")
    elif len(description) > 2048:
        _add(errors, "description", "Description must not exceed 2048 characters.")
    else:
        data["description"] = description

    # category (optional) ----------------------------------------------------
    category = body.get("category", None)
    if category is None:
        data["category"] = None
    elif not isinstance(category, str):
        _add(errors, "category", "Category must be a string.")
    elif len(category) > 64:
        _add(errors, "category", "Category must not exceed 64 characters.")
    else:
        data["category"] = category

    # price ------------------------------------------------------------------
    price = body.get("price", _MISSING)
    price_value = None
    if price is _MISSING or price is None:
        _add(errors, "price", "Price is required.")
    elif not _is_number(price):
        _add(errors, "price", "Price must be a number.")
    elif price < 0:
        _add(errors, "price", "Price must be greater than or equal to 0.")
    elif price > MAX_PRICE:
        _add(errors, "price", "Price must not exceed 9999999999999999.99.")
    else:
        price_value = price
        data["price"] = price

    # currency ---------------------------------------------------------------
    currency = body.get("currency", None)
    if price_value is not None and price_value > 0:
        if currency is None or (isinstance(currency, str) and currency == ""):
            _add(errors, "currency", "Currency is required when price is greater than 0.")
        elif not isinstance(currency, str) or len(currency) != 3:
            _add(errors, "currency", "Currency must be a valid ISO 4217 code (3 characters).")
        elif not is_supported(currency):
            _add(errors, "currency", "Currency must be a supported currency code.")
        else:
            data["currency"] = currency
    else:
        # Free article: currency is ignored/optional.
        data["currency"] = currency if isinstance(currency, str) else None

    return data


def validate_create(body, errors):
    return _validate_common(body, errors)


def validate_update(body, errors):
    data = _validate_common(body, errors)

    row_version = body.get("row_version", _MISSING)
    if row_version is _MISSING or row_version is None:
        _add(errors, "row_version", "Row version is required.")
    elif isinstance(row_version, bool) or not isinstance(row_version, int):
        _add(errors, "row_version", "Row version must be an integer.")
    elif row_version < 1:
        _add(errors, "row_version", "Row version must be a non-zero positive value.")
    else:
        data["row_version"] = row_version

    return data
