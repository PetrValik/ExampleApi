"""RFC 7807 ``application/problem+json`` responses.

Every error path in the API funnels through here so validation never leaks the
framework's default envelope — a validation failure is always 400 problem+json
with an ``errors`` object mapping field name -> list[str].
"""

from flask import jsonify, make_response

_RFC = "https://tools.ietf.org/html/rfc9110"


def _problem(status, title, detail=None, type_uri="about:blank", extra=None):
    payload = {"type": type_uri, "title": title, "status": status}
    if detail is not None:
        payload["detail"] = detail
    if extra:
        payload.update(extra)
    response = make_response(jsonify(payload), status)
    response.headers["Content-Type"] = "application/problem+json"
    return response


def validation_problem(errors):
    """400 with a per-field ``errors`` map."""
    return _problem(
        400,
        "One or more validation errors occurred.",
        type_uri=f"{_RFC}#section-15.5.1",
        extra={"errors": errors},
    )


def not_found_problem(detail="The requested resource was not found."):
    return _problem(404, "Not Found", detail, type_uri=f"{_RFC}#section-15.5.5")


def conflict_problem(detail="The resource was modified by another request."):
    return _problem(409, "Conflict", detail, type_uri=f"{_RFC}#section-15.5.10")


def unauthorized_problem(detail="Missing or invalid authentication token."):
    return _problem(401, "Unauthorized", detail, type_uri=f"{_RFC}#section-15.5.2")
