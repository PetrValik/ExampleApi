"""Application settings, read from environment variables with sane defaults.

Kept dependency-free (plain :mod:`os`) so importing the app never requires a
database or any secret to be present — the module simply falls back to safe
development defaults. Production values are injected via docker-compose.
"""

from __future__ import annotations

import os
from dataclasses import dataclass


@dataclass(frozen=True)
class Settings:
    """Strongly-typed view over the process environment."""

    # Async SQLAlchemy URL (asyncpg driver). Points at the compose "postgres" host.
    database_url: str = os.environ.get(
        "DATABASE_URL",
        "postgresql+asyncpg://postgres:postgres@postgres:5432/exampleapi",
    )

    # JWT signing/validation. The secret MUST be >= 32 chars for HS256.
    jwt_secret: str = os.environ.get(
        "JWT_SECRET",
        "CHANGE-THIS-BEFORE-RUNNING-IN-PRODUCTION-32chars-minimum",
    )
    jwt_issuer: str = os.environ.get("JWT_ISSUER", "ExampleApi")
    jwt_audience: str = os.environ.get("JWT_AUDIENCE", "ExampleApiClient")
    jwt_expiration_minutes: int = int(os.environ.get("JWT_EXPIRATION_MINUTES", "60"))

    # Demo credentials (documented stand-in for a real identity provider).
    demo_username: str = os.environ.get("DEMO_USERNAME", "admin")
    demo_password: str = os.environ.get("DEMO_PASSWORD", "admin")


settings = Settings()
