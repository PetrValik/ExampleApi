"""RFC 7807 problem+json helpers and domain exceptions.

Every error surface in this API is normalised to ``application/problem+json``.
Validation failures (400) additionally carry an ``errors`` object mapping a
field name to a list of human-readable messages.
"""

from __future__ import annotations

from fastapi import Request
from fastapi.responses import JSONResponse

PROBLEM_JSON = "application/problem+json"


class ValidationProblem(Exception):
    """Raised when request validation fails — rendered as 400 problem+json."""

    def __init__(self, errors: dict[str, list[str]]):
        self.errors = errors
        super().__init__("Validation failed")


class NotFoundProblem(Exception):
    """Raised when a resource cannot be found — rendered as 404 problem+json."""

    def __init__(self, detail: str):
        self.detail = detail
        super().__init__(detail)


class ConflictProblem(Exception):
    """Raised on an optimistic-concurrency conflict — rendered as 409 problem+json."""

    def __init__(self, detail: str):
        self.detail = detail
        super().__init__(detail)


def problem_response(
    status: int,
    title: str,
    *,
    detail: str | None = None,
    errors: dict[str, list[str]] | None = None,
) -> JSONResponse:
    """Build a problem+json JSONResponse with the correct media type."""
    body: dict[str, object] = {
        "type": f"https://httpstatuses.com/{status}",
        "title": title,
        "status": status,
    }
    if detail is not None:
        body["detail"] = detail
    if errors is not None:
        body["errors"] = errors
    return JSONResponse(status_code=status, content=body, media_type=PROBLEM_JSON)


def validation_problem_response(errors: dict[str, list[str]]) -> JSONResponse:
    return problem_response(
        400,
        "One or more validation errors occurred.",
        errors=errors,
    )


# --- Exception handlers (registered on the app in main.py) -------------------


async def validation_problem_handler(_: Request, exc: ValidationProblem) -> JSONResponse:
    return validation_problem_response(exc.errors)


async def not_found_handler(_: Request, exc: NotFoundProblem) -> JSONResponse:
    return problem_response(404, "Not Found", detail=exc.detail)


async def conflict_handler(_: Request, exc: ConflictProblem) -> JSONResponse:
    return problem_response(409, "Conflict", detail=exc.detail)


async def request_validation_handler(_: Request, exc) -> JSONResponse:
    """Override FastAPI's default 422 envelope.

    FastAPI raises ``RequestValidationError`` for malformed/mistyped bodies
    (e.g. ``price`` not a number). Convert its error list into the same
    400 problem+json shape used by our explicit validators, keyed by field name.
    """
    errors: dict[str, list[str]] = {}
    for err in exc.errors():
        loc = err.get("loc", ())
        # Drop the leading "body"/"query" segment; use the last string part as
        # the field key so it contains the offending field's name.
        parts = [str(p) for p in loc if p not in ("body", "query", "path")]
        field = parts[-1] if parts else "request"
        errors.setdefault(field, []).append(err.get("msg", "Invalid value."))
    if not errors:
        errors["request"] = ["The request is invalid."]
    return validation_problem_response(errors)
