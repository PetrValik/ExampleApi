"""Async SQLAlchemy engine, session factory and declarative base.

The engine is created lazily by SQLAlchemy — constructing it does *not* open a
connection, so importing this module (and therefore the app) works without a
running database. Actual connectivity is established at startup, with retries.
"""

from __future__ import annotations

from sqlalchemy.ext.asyncio import (
    AsyncSession,
    async_sessionmaker,
    create_async_engine,
)
from sqlalchemy.orm import DeclarativeBase

from app.config import settings

engine = create_async_engine(settings.database_url, echo=False, future=True)

SessionLocal = async_sessionmaker(
    bind=engine,
    class_=AsyncSession,
    expire_on_commit=False,
)


class Base(DeclarativeBase):
    """Declarative base for all ORM models."""


async def get_session() -> AsyncSession:
    """FastAPI dependency yielding a scoped async session."""
    async with SessionLocal() as session:
        yield session
