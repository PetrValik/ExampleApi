"""Pydantic v2 request/response models.

Request models are intentionally *lenient* on business rules (types only) so
that the explicit validators in :mod:`app.validation` own the field-keyed error
messages and every failure lands as 400 problem+json rather than Pydantic's
default 422 envelope. Genuine type/parse failures still route through the
overridden ``RequestValidationError`` handler and also become 400.
"""

from __future__ import annotations

from pydantic import BaseModel, ConfigDict


class TokenRequest(BaseModel):
    username: str = ""
    password: str = ""


class TokenResponse(BaseModel):
    token: str
    expiresAt: str  # noqa: N815 — camelCase is contractually pinned


class ArticleRequest(BaseModel):
    """Create body. ``price`` uses ``None`` as the "absent" sentinel so 0 is valid."""

    model_config = ConfigDict(extra="ignore")

    name: str = ""
    description: str = ""
    category: str | None = None
    price: float | None = None
    currency: str | None = None


class UpdateArticleRequest(ArticleRequest):
    """Update body — adds the required optimistic-concurrency token."""

    row_version: int | None = None


class ArticleResponse(BaseModel):
    """Article wire shape (snake_case, exactly as pinned by the contract)."""

    article_id: int
    name: str
    description: str
    category: str | None
    price: float
    currency: str | None
    row_version: int


class PagedArticleResponse(BaseModel):
    """Pagination wrapper (camelCase, exactly as pinned by the contract)."""

    items: list[ArticleResponse]
    page: int
    pageSize: int  # noqa: N815
    totalCount: int  # noqa: N815
    totalPages: int  # noqa: N815
    hasPrevious: bool  # noqa: N815
    hasNext: bool  # noqa: N815
