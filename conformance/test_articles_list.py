"""GET /api/articles — filtering and pagination, scoped to freshly-seeded rows."""

from conftest import unique


def test_filter_by_category_returns_only_that_category(client, make_article):
    category = unique("cat")
    for _ in range(3):
        make_article(category=category)

    response = client.get("/api/articles", params={"category": category, "pageSize": 100})

    assert response.status_code == 200
    body = response.json()
    assert body["totalCount"] == 3
    assert len(body["items"]) == 3
    assert all(item["category"] == category for item in body["items"])


def test_filter_by_name_partial_and_case_insensitive(client, make_article):
    marker = unique("Zzq")  # unlikely to collide
    make_article(name=f"prefix-{marker}-suffix")

    response = client.get("/api/articles", params={"name": marker.lower(), "pageSize": 100})

    assert response.status_code == 200
    body = response.json()
    assert body["totalCount"] == 1
    assert marker in body["items"][0]["name"]


def test_pagination_metadata(client, make_article):
    category = unique("page")
    for _ in range(3):
        make_article(category=category)

    first = client.get(
        "/api/articles", params={"category": category, "page": 1, "pageSize": 2}
    ).json()

    assert first["page"] == 1
    assert first["pageSize"] == 2
    assert first["totalCount"] == 3
    assert first["totalPages"] == 2
    assert len(first["items"]) == 2
    assert first["hasPrevious"] is False
    assert first["hasNext"] is True

    second = client.get(
        "/api/articles", params={"category": category, "page": 2, "pageSize": 2}
    ).json()

    assert second["page"] == 2
    assert len(second["items"]) == 1
    assert second["hasPrevious"] is True
    assert second["hasNext"] is False


def test_page_size_is_clamped_to_100(client):
    response = client.get("/api/articles", params={"pageSize": 1000})

    assert response.status_code == 200
    assert response.json()["pageSize"] == 100
