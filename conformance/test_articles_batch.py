"""POST /api/articles-concurrent — batch creation."""

from conftest import unique


def test_batch_create_returns_all_with_ids(client):
    category = unique("batch")
    payload = [
        {
            "name": unique("b"),
            "description": "batch item",
            "category": category,
            "price": 1.00,
            "currency": "USD",
        }
        for _ in range(3)
    ]

    response = client.post("/api/articles-concurrent", json=payload)

    assert response.status_code == 201
    created = response.json()
    assert len(created) == 3
    assert all(item["article_id"] > 0 for item in created)
    assert all(item["category"] == category for item in created)


def test_batch_with_one_invalid_item_returns_400(client):
    payload = [
        {"name": unique("ok"), "description": "ok", "price": 1.0, "currency": "USD"},
        {"name": "", "description": "bad", "price": 1.0, "currency": "USD"},
    ]

    response = client.post("/api/articles-concurrent", json=payload)

    assert response.status_code == 400
