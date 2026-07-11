"""GET /health — liveness, anonymous."""


def test_health_returns_healthy(anon_client):
    response = anon_client.get("/health")

    assert response.status_code == 200
    assert response.json()["status"] == "healthy"
