"""JWT token issuance (anonymous).

Demo credentials ``admin`` / ``admin`` are hardcoded as a stand-in for a real
identity provider. Any other credentials -> 401. The issued token is an HS256
JWT with a ``name`` claim, issuer, audience and expiry, all driven from config.
"""

from datetime import datetime, timedelta, timezone

from flask import Blueprint, current_app, jsonify, request
from flask_jwt_extended import create_access_token

from ..problems import unauthorized_problem

auth_bp = Blueprint("auth", __name__)

DEMO_USERNAME = "admin"
DEMO_PASSWORD = "admin"


def _iso_utc(moment):
    """Format as an RFC 3339 UTC instant, e.g. 2026-07-12T10:00:00Z."""
    return moment.astimezone(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


@auth_bp.post("/auth/token")
def get_token():
    body = request.get_json(silent=True) or {}
    username = body.get("username")
    password = body.get("password")

    if username != DEMO_USERNAME or password != DEMO_PASSWORD:
        return unauthorized_problem("Invalid username or password.")

    minutes = current_app.config["JWT_EXPIRATION_MINUTES"]
    expires_at = datetime.now(timezone.utc) + timedelta(minutes=minutes)

    token = create_access_token(
        identity=username,
        additional_claims={"name": username},
    )

    return jsonify({"token": token, "expiresAt": _iso_utc(expires_at)}), 200
