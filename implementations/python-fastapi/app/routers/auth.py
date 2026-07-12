"""JWT token issuance (anonymous)."""

from __future__ import annotations

from fastapi import APIRouter, Response

from app.config import settings
from app.schemas import TokenRequest
from app.security import create_token

router = APIRouter(tags=["Auth"])


@router.post("/auth/token")
async def get_token(body: TokenRequest, response: Response):
    """Return a signed JWT for the demo user; any other credentials → 401."""
    if body.username != settings.demo_username or body.password != settings.demo_password:
        response.status_code = 401
        return None

    token, expires_at = create_token(body.username)
    return {"token": token, "expiresAt": expires_at}
