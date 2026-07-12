"""FastAPI application wiring: routers, problem+json handlers, DB bootstrap."""

from __future__ import annotations

import asyncio
import logging
from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.exceptions import RequestValidationError

from app.database import Base, engine
from app.problem import (
    ConflictProblem,
    NotFoundProblem,
    ValidationProblem,
    conflict_handler,
    not_found_handler,
    request_validation_handler,
    validation_problem_handler,
)
from app.routers import articles, auth, health

logger = logging.getLogger("exampleapi")

# Import models so their tables are registered on Base.metadata before create_all.
from app import models  # noqa: E402,F401


async def _init_db(max_retries: int = 15, base_delay: float = 2.0) -> None:
    """Create tables, retrying while the database comes up (compose healthcheck aside)."""
    delay = base_delay
    for attempt in range(1, max_retries + 1):
        try:
            async with engine.begin() as conn:
                await conn.run_sync(Base.metadata.create_all)
            logger.info("Database initialised successfully")
            return
        except Exception as exc:  # noqa: BLE001 — retry any connectivity error
            if attempt == max_retries:
                logger.critical("DB init failed after %s attempts: %s", max_retries, exc)
                raise
            logger.warning(
                "DB init attempt %s/%s failed (%s); retrying in %.1fs",
                attempt, max_retries, exc, delay,
            )
            await asyncio.sleep(delay)
            delay = min(delay * 2, 30.0)


@asynccontextmanager
async def lifespan(_: FastAPI):
    await _init_db()
    yield


app = FastAPI(
    title="Example API — Python FastAPI",
    version="1.0.0",
    lifespan=lifespan,
)

# Normalise every validation/error surface to application/problem+json.
app.add_exception_handler(RequestValidationError, request_validation_handler)
app.add_exception_handler(ValidationProblem, validation_problem_handler)
app.add_exception_handler(NotFoundProblem, not_found_handler)
app.add_exception_handler(ConflictProblem, conflict_handler)

app.include_router(health.router)
app.include_router(auth.router)
app.include_router(articles.router)
