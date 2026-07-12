"""POST /auth/token and the protected-endpoint gate."""

import pytest


def test_valid_credentials_return_token(anon_client):
    response = anon_client.post(
        "/auth/token", json={"username": "admin", "password": "admin"}
    )

    assert response.status_code == 200
    body = response.json()
    assert body["token"]
    assert body["expiresAt"]


@pytest.mark.parametrize(
    "credentials",
    [
        {"username": "admin", "password": "wrong"},
        {"username": "nobody", "password": "admin"},
        {"username": "", "password": ""},
    ],
)
def test_invalid_credentials_return_401(anon_client, credentials):
    response = anon_client.post("/auth/token", json=credentials)

    assert response.status_code == 401


def test_protected_endpoint_without_token_returns_401(anon_client):
    response = anon_client.get("/api/articles")

    assert response.status_code == 401
