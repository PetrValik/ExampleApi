"""Request validation on POST /api/articles — every failing rule returns 400 problem+json."""

import pytest

from conftest import unique


def _valid_body(**overrides) -> dict:
    body = {
        "name": unique("valid"),
        "description": "a valid description",
        "category": "Electronics",
        "price": 9.99,
        "currency": "USD",
    }
    body.update(overrides)
    return body


@pytest.mark.parametrize(
    "overrides",
    [
        pytest.param({"name": ""}, id="empty-name"),
        pytest.param({"name": "A" * 65}, id="name-too-long"),
        pytest.param({"description": ""}, id="empty-description"),
        pytest.param({"description": "A" * 2049}, id="description-too-long"),
        pytest.param({"category": "A" * 65}, id="category-too-long"),
        pytest.param({"price": -1}, id="negative-price"),
        pytest.param({"price": 10, "currency": None}, id="price-without-currency"),
        pytest.param({"price": 10, "currency": "US"}, id="currency-wrong-length"),
        pytest.param({"price": 10, "currency": "BBB"}, id="unsupported-currency"),
    ],
)
def test_invalid_create_returns_400(client, overrides):
    response = client.post("/api/articles", json=_valid_body(**overrides))

    assert response.status_code == 400


def test_validation_body_is_problem_details(client):
    response = client.post("/api/articles", json=_valid_body(name=""))

    assert response.status_code == 400
    assert "application/problem+json" in response.headers.get("content-type", "")
    body = response.json()
    assert "errors" in body
    # the offending field is reported
    assert any("name" in key.lower() for key in body["errors"])
