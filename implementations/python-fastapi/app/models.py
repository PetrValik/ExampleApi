"""SQLAlchemy ORM models."""

from __future__ import annotations

from decimal import Decimal

from sqlalchemy import Integer, Numeric, String
from sqlalchemy.orm import Mapped, mapped_column

from app.database import Base


class Article(Base):
    """A shop article/product.

    ``version`` is a portable optimistic-concurrency token: it starts at 1 on
    insert and is incremented on every update. It is surfaced to clients as
    ``row_version``; a PUT carrying a stale value is rejected with 409.
    """

    __tablename__ = "articles"

    article_id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    name: Mapped[str] = mapped_column(String(64), nullable=False)
    description: Mapped[str] = mapped_column(String(2048), nullable=False)
    category: Mapped[str | None] = mapped_column(String(64), nullable=True)
    price: Mapped[Decimal] = mapped_column(Numeric(18, 2), nullable=False, default=Decimal("0"))
    currency: Mapped[str | None] = mapped_column(String(3), nullable=True)
    version: Mapped[int] = mapped_column(Integer, nullable=False, default=1)
