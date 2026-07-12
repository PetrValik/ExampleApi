"""RFC 7807 problem+json normalisation.

DRF's default error envelope (``{"field": ["msg"]}`` at 400, ``{"detail": ...}``
for 404/409, or a raw 422) is replaced with ``application/problem+json``:

* validation (400) -> ``{type,title,status,errors:{field:[msgs]}}``
* not found / conflict / other (>=400) -> ``{type,title,status,detail}``
* 401 -> left as DRF's response (status only; no body is contractually needed),
  preserving the ``WWW-Authenticate`` header.
"""

import json

from django.http import HttpResponse
from rest_framework.exceptions import APIException, ValidationError
from rest_framework.views import exception_handler as drf_exception_handler

_TITLES = {
    400: "One or more validation errors occurred.",
    404: "The requested resource was not found.",
    405: "Method not allowed.",
    409: "The resource was modified by another request.",
    415: "Unsupported media type.",
}


def _problem(body: dict, status_code: int) -> HttpResponse:
    return HttpResponse(
        json.dumps(body),
        status=status_code,
        content_type="application/problem+json",
    )


def _flatten_messages(value) -> list[str]:
    """Coerce a DRF error value into a flat list of strings."""
    messages: list[str] = []
    if isinstance(value, list):
        for item in value:
            if isinstance(item, (list, dict)):
                messages.extend(_flatten_messages(item))
            else:
                messages.append(str(item))
    elif isinstance(value, dict):
        for nested in value.values():
            messages.extend(_flatten_messages(nested))
    else:
        messages.append(str(value))
    return messages


def _to_errors(detail) -> dict:
    """Normalise a ValidationError detail into a ``{field: [messages]}`` map."""
    errors: dict[str, list[str]] = {}
    if isinstance(detail, dict):
        for key, value in detail.items():
            errors[str(key)] = _flatten_messages(value)
    elif isinstance(detail, list):
        # Either a list of per-item dicts (many=True) or a bare message list.
        for index, item in enumerate(detail):
            if isinstance(item, dict):
                for key, value in item.items():
                    errors[f"[{index}].{key}"] = _flatten_messages(value)
            elif item:  # skip empty {} placeholders rendered as falsy
                errors.setdefault("non_field_errors", []).extend(
                    _flatten_messages(item)
                )
    else:
        errors["non_field_errors"] = [str(detail)]
    if not errors:
        errors["non_field_errors"] = ["Invalid request."]
    return errors


def problem_exception_handler(exc, context):
    response = drf_exception_handler(exc, context)
    if response is None:
        # Unhandled exception — let Django produce its 500.
        return None

    status_code = response.status_code

    # 401: keep DRF's response (carries WWW-Authenticate); status is all tests need.
    if status_code == 401:
        return response

    if isinstance(exc, ValidationError):
        return _problem(
            {
                "type": "about:blank",
                "title": _TITLES[400],
                "status": 400,
                "errors": _to_errors(exc.detail),
            },
            400,
        )

    detail = getattr(exc, "detail", None)
    if isinstance(detail, (list, dict)):
        detail_text = "; ".join(_flatten_messages(detail))
    elif detail is not None:
        detail_text = str(detail)
    else:
        detail_text = _TITLES.get(status_code, "An error occurred.")

    return _problem(
        {
            "type": "about:blank",
            "title": _TITLES.get(status_code, "An error occurred."),
            "status": status_code,
            "detail": detail_text,
        },
        status_code,
    )


class ConflictException(APIException):
    """Raised on an optimistic-concurrency conflict — maps to 409."""

    status_code = 409
    default_detail = "The resource was modified by another request. Please retry."
    default_code = "conflict"
