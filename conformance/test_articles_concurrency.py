"""PUT /api/articles/{id} optimistic concurrency — a stale row_version yields 409."""


def test_stale_row_version_returns_409(client, make_article):
    created = make_article()
    article_id = created["article_id"]
    stale_version = created["row_version"]

    update = {
        "name": created["name"],
        "description": "first update",
        "category": created["category"],
        "price": created["price"],
        "currency": created["currency"],
        "row_version": stale_version,
    }

    # First update succeeds and advances the row version.
    first = client.put(f"/api/articles/{article_id}", json=update)
    assert first.status_code == 200

    # Reusing the now-stale row version is a conflicting write.
    update["description"] = "second update with a stale version"
    second = client.put(f"/api/articles/{article_id}", json=update)
    assert second.status_code == 409


def test_missing_row_version_returns_400(client, make_article):
    created = make_article()
    article_id = created["article_id"]

    update = {
        "name": created["name"],
        "description": "no row version supplied",
        "category": created["category"],
        "price": created["price"],
        "currency": created["currency"],
        # row_version deliberately omitted — the validator requires it
    }

    response = client.put(f"/api/articles/{article_id}", json=update)
    assert response.status_code == 400
