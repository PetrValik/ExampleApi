"""JWT bearer authentication and token issuance.

The demo has no user table, so tokens are minted and verified directly with
PyJWT (the same JWT engine that backs ``djangorestframework-simplejwt``). This
keeps full control over the ``name`` / issuer / audience / expiry claims that
the contract requires, without coupling to a Django user model.
"""

from datetime import datetime, timedelta, timezone

import jwt
from django.conf import settings
from rest_framework.authentication import BaseAuthentication, get_authorization_header
from rest_framework.exceptions import AuthenticationFailed

ALGORITHM = "HS256"


class DemoUser:
    """A minimal authenticated principal (no database backing)."""

    is_authenticated = True
    is_anonymous = False

    def __init__(self, username: str):
        self.username = username

    def __str__(self) -> str:  # pragma: no cover - cosmetic
        return self.username


def issue_token(username: str) -> tuple[str, datetime]:
    """Mint an HS256 JWT for ``username``; return ``(token, expires_at)``."""
    now = datetime.now(timezone.utc)
    expires_at = now + timedelta(minutes=settings.JWT_EXPIRATION_MINUTES)
    payload = {
        "name": username,
        "sub": username,
        "iss": settings.JWT_ISSUER,
        "aud": settings.JWT_AUDIENCE,
        "iat": int(now.timestamp()),
        "exp": int(expires_at.timestamp()),
    }
    token = jwt.encode(payload, settings.JWT_SECRET, algorithm=ALGORITHM)
    return token, expires_at


class JWTAuthentication(BaseAuthentication):
    """Validates ``Authorization: Bearer <jwt>`` and verifies iss/aud/exp."""

    keyword = b"bearer"

    def authenticate(self, request):
        header = get_authorization_header(request).split()
        if not header or header[0].lower() != self.keyword:
            # No bearer credentials — let IsAuthenticated reject with 401.
            return None
        if len(header) != 2:
            raise AuthenticationFailed("Invalid Authorization header.")

        raw_token = header[1].decode("utf-8", errors="ignore")
        try:
            payload = jwt.decode(
                raw_token,
                settings.JWT_SECRET,
                algorithms=[ALGORITHM],
                audience=settings.JWT_AUDIENCE,
                issuer=settings.JWT_ISSUER,
            )
        except jwt.PyJWTError as exc:
            raise AuthenticationFailed("Invalid or expired token.") from exc

        return DemoUser(payload.get("name", "unknown")), raw_token

    def authenticate_header(self, request):
        # Presence of this header makes IsAuthenticated failures return 401.
        return "Bearer"
