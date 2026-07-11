"""Shared fixtures for the conformance suite.

The suite is black-box: it drives whatever implementation is listening at ``BASE_URL``
(default ``http://localhost:8080``) purely over HTTP. Every test seeds its own uniquely
named data and scopes its assertions to those rows, so the suite never depends on a clean
database and never asserts on global counts — it can run against a shared, already-populated
instance without flaking.
"""

import os
import uuid

import httpx
import pytest

BASE_URL = os.environ.get("BASE_URL", "http://localhost:8080")
CREDENTIALS = {"username": "admin", "password": "admin"}
TIMEOUT = httpx.Timeout(30.0)


def unique(prefix: str = "conf") -> str:
    """A collision-free identifier so tests never clash with existing rows."""
    return f"{prefix}-{uuid.uuid4().hex[:12]}"


@pytest.fixture(scope="session")
def base_url() -> str:
    return BASE_URL


@pytest.fixture(scope="session")
def token(base_url: str) -> str:
    """Obtain a bearer token once for the whole session."""
    with httpx.Client(base_url=base_url, timeout=TIMEOUT) as client:
        response = client.post("/auth/token", json=CREDENTIALS)
        response.raise_for_status()
        return response.json()["token"]


@pytest.fixture
def client(base_url: str, token: str):
    """Authenticated HTTP client."""
    with httpx.Client(
        base_url=base_url,
        timeout=TIMEOUT,
        headers={"Authorization": f"Bearer {token}"},
    ) as client:
        yield client


@pytest.fixture
def anon_client(base_url: str):
    """Unauthenticated HTTP client."""
    with httpx.Client(base_url=base_url, timeout=TIMEOUT) as client:
        yield client


@pytest.fixture
def make_article(client: httpx.Client):
    """Factory that creates an article (unique by default) and returns its response body."""

    def _make(**overrides) -> dict:
        body = {
            "name": unique("article"),
            "description": "conformance fixture article",
            "category": unique("cat"),
            "price": 9.99,
            "currency": "USD",
        }
        body.update(overrides)
        response = client.post("/api/articles", json=body)
        assert response.status_code == 201, response.text
        return response.json()

    return _make
