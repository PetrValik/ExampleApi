"""Article CRUD, listing and batch creation — all JWT-protected."""

from __future__ import annotations

import math
from decimal import Decimal

from fastapi import APIRouter, Depends, Query, Request, Response
from fastapi.responses import JSONResponse
from sqlalchemy import func, select
from sqlalchemy.ext.asyncio import AsyncSession

from app.database import get_session
from app.models import Article
from app.problem import ConflictProblem, NotFoundProblem, ValidationProblem
from app.schemas import ArticleRequest, UpdateArticleRequest
from app.security import require_auth
from app.validation import validate_create, validate_update

# require_auth guards every route on this router → missing/invalid token = 401.
router = APIRouter(prefix="/api", tags=["Articles"], dependencies=[Depends(require_auth)])

MAX_PAGE_SIZE = 100


def to_response(article: Article) -> dict:
    """Map an ORM entity to the exact snake_case wire shape."""
    return {
        "article_id": article.article_id,
        "name": article.name,
        "description": article.description,
        "category": article.category,
        "price": float(article.price) if article.price is not None else 0.0,
        "currency": article.currency,
        "row_version": article.version,
    }


def _new_article(req: ArticleRequest) -> Article:
    return Article(
        name=req.name,
        description=req.description,
        category=req.category,
        price=Decimal(str(req.price if req.price is not None else 0)),
        currency=req.currency,
        version=1,
    )


def _escape_like(pattern: str) -> str:
    """Escape LIKE/ILIKE wildcards so user input matches literally."""
    return pattern.replace("\\", "\\\\").replace("%", "\\%").replace("_", "\\_")


@router.get("/articles")
async def list_articles(
    session: AsyncSession = Depends(get_session),
    name: str | None = Query(default=None),
    category: str | None = Query(default=None),
    page: int = Query(default=1),
    pageSize: int = Query(default=10),  # noqa: N803 — camelCase query param is pinned
) -> JSONResponse:
    query = select(Article)
    count_query = select(func.count()).select_from(Article)

    if name:
        pattern = f"%{_escape_like(name)}%"
        query = query.where(Article.name.ilike(pattern, escape="\\"))
        count_query = count_query.where(Article.name.ilike(pattern, escape="\\"))
    if category:
        query = query.where(Article.category == category)
        count_query = count_query.where(Article.category == category)

    total_count = (await session.execute(count_query)).scalar_one()

    page = max(1, page)
    size = min(max(1, pageSize), MAX_PAGE_SIZE)

    rows = (
        await session.execute(
            query.order_by(Article.article_id).offset((page - 1) * size).limit(size)
        )
    ).scalars().all()

    total_pages = math.ceil(total_count / size) if size else 0
    body = {
        "items": [to_response(a) for a in rows],
        "page": page,
        "pageSize": size,
        "totalCount": total_count,
        "totalPages": total_pages,
        "hasPrevious": page > 1,
        "hasNext": page < total_pages,
    }
    return JSONResponse(status_code=200, content=body)


@router.post("/articles")
async def create_article(
    body: ArticleRequest,
    session: AsyncSession = Depends(get_session),
) -> JSONResponse:
    errors = validate_create(body)
    if errors:
        raise ValidationProblem(errors)

    article = _new_article(body)
    session.add(article)
    await session.commit()
    await session.refresh(article)

    payload = to_response(article)
    return JSONResponse(
        status_code=201,
        content=payload,
        headers={"Location": f"/api/articles/{article.article_id}"},
    )


@router.get("/articles/{article_id}")
async def get_article(
    article_id: int,
    session: AsyncSession = Depends(get_session),
) -> JSONResponse:
    article = await session.get(Article, article_id)
    if article is None:
        raise NotFoundProblem(f"Article with ID {article_id} was not found.")
    return JSONResponse(status_code=200, content=to_response(article))


@router.put("/articles/{article_id}")
async def update_article(
    article_id: int,
    body: UpdateArticleRequest,
    session: AsyncSession = Depends(get_session),
) -> JSONResponse:
    errors = validate_update(body)
    if errors:
        raise ValidationProblem(errors)

    article = await session.get(Article, article_id)
    if article is None:
        raise NotFoundProblem(f"Article with ID {article_id} was not found.")

    # Optimistic concurrency: the supplied row_version must match the current one.
    if article.version != body.row_version:
        raise ConflictProblem(
            f"Article with ID {article_id} was modified by another request. Please retry."
        )

    article.name = body.name
    article.description = body.description
    article.category = body.category
    article.price = Decimal(str(body.price if body.price is not None else 0))
    article.currency = body.currency
    article.version += 1  # advance the concurrency token

    await session.commit()
    await session.refresh(article)
    return JSONResponse(status_code=200, content=to_response(article))


@router.delete("/articles/{article_id}")
async def delete_article(
    article_id: int,
    session: AsyncSession = Depends(get_session),
) -> Response:
    article = await session.get(Article, article_id)
    if article is None:
        raise NotFoundProblem(f"Article with ID {article_id} was not found.")
    await session.delete(article)
    await session.commit()
    # 204 must carry NO body — JSONResponse(content=None) emits `null`, which is an
    # illegal 204 body and desyncs HTTP keep-alive (the next request on the connection
    # then fails with "Server disconnected"). A bare Response sends the correct empty 204.
    return Response(status_code=204)


@router.post("/articles-concurrent")
async def batch_create_articles(
    request: Request,
    session: AsyncSession = Depends(get_session),
) -> JSONResponse:
    """Create many articles at once. Empty array, any invalid item, or >100 → 400."""
    raw = await request.json()
    if not isinstance(raw, list):
        raise ValidationProblem({"request": ["A JSON array of articles is required."]})
    if len(raw) == 0:
        raise ValidationProblem({"request": ["At least one article is required."]})
    if len(raw) > 100:
        raise ValidationProblem({"request": ["Cannot create more than 100 articles at once."]})

    parsed: list[ArticleRequest] = []
    errors: dict[str, list[str]] = {}
    for index, item in enumerate(raw):
        try:
            req = ArticleRequest.model_validate(item)
        except Exception:  # noqa: BLE001 — malformed item → 400
            errors.setdefault(f"[{index}]", []).append("Invalid article.")
            continue
        item_errors = validate_create(req)
        if item_errors:
            for key, messages in item_errors.items():
                errors.setdefault(f"[{index}].{key}", []).extend(messages)
        else:
            parsed.append(req)

    if errors:
        raise ValidationProblem(errors)

    articles = [_new_article(req) for req in parsed]
    session.add_all(articles)
    await session.commit()
    for article in articles:
        await session.refresh(article)

    return JSONResponse(
        status_code=201,
        content=[to_response(a) for a in articles],
    )
