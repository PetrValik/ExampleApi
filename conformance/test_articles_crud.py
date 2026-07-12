"""The create → read → update → delete lifecycle for a single article."""

from conftest import unique


def test_create_returns_201_with_location_and_body(client):
    name = unique("kbd")
    body = {
        "name": name,
        "description": "A compact wireless keyboard.",
        "category": "Electronics",
        "price": 49.99,
        "currency": "USD",
    }

    response = client.post("/api/articles", json=body)

    assert response.status_code == 201
    assert "Location" in response.headers
    created = response.json()
    assert created["article_id"] > 0
    assert created["name"] == name
    assert created["description"] == body["description"]
    assert created["price"] == 49.99
    assert created["currency"] == "USD"
    assert response.headers["Location"].endswith(str(created["article_id"]))


def test_full_lifecycle(client, make_article):
    created = make_article()
    article_id = created["article_id"]

    # read
    got = client.get(f"/api/articles/{article_id}")
    assert got.status_code == 200
    assert got.json()["article_id"] == article_id

    # update
    new_name = unique("renamed")
    update_body = {
        "name": new_name,
        "description": "updated description",
        "category": "Books",
        "price": 12.50,
        "currency": "EUR",
        "row_version": created["row_version"],
    }
    updated = client.put(f"/api/articles/{article_id}", json=update_body)
    assert updated.status_code == 200
    updated_body = updated.json()
    assert updated_body["name"] == new_name
    assert updated_body["category"] == "Books"
    assert updated_body["currency"] == "EUR"

    # delete
    deleted = client.delete(f"/api/articles/{article_id}")
    assert deleted.status_code == 204

    # gone
    assert client.get(f"/api/articles/{article_id}").status_code == 404


def test_get_unknown_returns_404(client):
    response = client.get("/api/articles/999999999")

    assert response.status_code == 404


def test_delete_unknown_returns_404(client):
    response = client.delete("/api/articles/999999999")

    assert response.status_code == 404


def test_free_article_needs_no_currency(client):
    body = {
        "name": unique("free"),
        "description": "A free promotional item.",
        "price": 0,
        "currency": None,
    }

    response = client.post("/api/articles", json=body)

    assert response.status_code == 201
    created = response.json()
    assert created["price"] == 0
    assert created["currency"] is None
