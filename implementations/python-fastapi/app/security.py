"""JWT issuance/verification (HS256, PyJWT) and the bearer-auth dependency."""

from __future__ import annotations

from datetime import datetime, timedelta, timezone

import jwt
from fastapi import Header, HTTPException

from app.config import settings


def create_token(username: str) -> tuple[str, str]:
    """Issue an HS256 JWT for ``username``.

    Returns ``(token, expires_at_iso)`` where the ISO string is UTC with a
    trailing ``Z`` (e.g. ``2026-07-12T10:00:00Z``).
    """
    now = datetime.now(timezone.utc).replace(microsecond=0)
    expires_at = now + timedelta(minutes=settings.jwt_expiration_minutes)

    payload = {
        "name": username,
        "sub": username,
        "iss": settings.jwt_issuer,
        "aud": settings.jwt_audience,
        "iat": int(now.timestamp()),
        "exp": int(expires_at.timestamp()),
    }
    token = jwt.encode(payload, settings.jwt_secret, algorithm="HS256")
    expires_at_iso = expires_at.strftime("%Y-%m-%dT%H:%M:%SZ")
    return token, expires_at_iso


def _verify_token(token: str) -> dict:
    """Validate signature, issuer, audience and expiry. Raises on any failure."""
    return jwt.decode(
        token,
        settings.jwt_secret,
        algorithms=["HS256"],
        issuer=settings.jwt_issuer,
        audience=settings.jwt_audience,
    )


def require_auth(authorization: str | None = Header(default=None)) -> dict:
    """FastAPI dependency enforcing ``Authorization: Bearer <jwt>``.

    Missing or invalid credentials yield 401 (never 403) — matching the
    contract's protected-endpoint gate.
    """
    if not authorization or not authorization.lower().startswith("bearer "):
        raise HTTPException(status_code=401, detail="Missing or invalid bearer token.")
    token = authorization.split(" ", 1)[1].strip()
    try:
        return _verify_token(token)
    except Exception as exc:  # noqa: BLE001 — any JWT error is a 401
        raise HTTPException(
            status_code=401, detail="Missing or invalid bearer token."
        ) from exc
