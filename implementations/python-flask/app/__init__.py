"""Flask application factory.

Classic sync WSGI stack: Flask + Flask-SQLAlchemy + flask-jwt-extended, served by
gunicorn in the container. Blueprints per resource, manual validation, and a
shared problem+json error surface.
"""

import os
import time
from datetime import timedelta

from flask import Flask

from .blueprints.articles import articles_bp
from .blueprints.auth import auth_bp
from .blueprints.health import health_bp
from .extensions import db, jwt
from .problems import unauthorized_problem

DEFAULT_DB_URL = "postgresql+psycopg://postgres:postgres@localhost:5432/exampleapi"
DEFAULT_SECRET = "CHANGE-THIS-IN-PRODUCTION-use-a-long-random-secret-key-min-32chars"
DEFAULT_ISSUER = "ExampleApi"
DEFAULT_AUDIENCE = "ExampleApiClient"


def _normalize_db_url(url):
    """Force psycopg (v3) driver for Postgres URLs; leave others untouched."""
    if url.startswith("postgres://"):
        return "postgresql+psycopg://" + url[len("postgres://"):]
    if url.startswith("postgresql://"):
        return "postgresql+psycopg://" + url[len("postgresql://"):]
    return url


def _configure(app, overrides):
    db_url = _normalize_db_url(os.environ.get("DATABASE_URL", DEFAULT_DB_URL))
    app.config["SQLALCHEMY_DATABASE_URI"] = db_url
    app.config["SQLALCHEMY_TRACK_MODIFICATIONS"] = False
    app.config["SQLALCHEMY_ENGINE_OPTIONS"] = {"pool_pre_ping": True}

    minutes = int(os.environ.get("JWT_EXPIRATION_MINUTES", "60"))
    issuer = os.environ.get("JWT_ISSUER", DEFAULT_ISSUER)
    audience = os.environ.get("JWT_AUDIENCE", DEFAULT_AUDIENCE)

    app.config["JWT_SECRET_KEY"] = os.environ.get("JWT_SECRET_KEY", DEFAULT_SECRET)
    app.config["JWT_ALGORITHM"] = "HS256"
    app.config["JWT_ACCESS_TOKEN_EXPIRES"] = timedelta(minutes=minutes)
    # Issuer / audience are stamped on encode and verified on decode.
    app.config["JWT_ENCODE_ISSUER"] = issuer
    app.config["JWT_DECODE_ISSUER"] = issuer
    app.config["JWT_ENCODE_AUDIENCE"] = audience
    app.config["JWT_DECODE_AUDIENCE"] = audience
    # Kept for the /auth/token handler to compute expiresAt.
    app.config["JWT_EXPIRATION_MINUTES"] = minutes

    if overrides:
        app.config.update(overrides)


def _register_jwt_handlers():
    """Normalise every JWT failure to 401 problem+json."""

    @jwt.unauthorized_loader
    def _missing_token(reason):
        return unauthorized_problem(reason)

    @jwt.invalid_token_loader
    def _invalid_token(reason):
        return unauthorized_problem(reason)

    @jwt.expired_token_loader
    def _expired_token(_header, _payload):
        return unauthorized_problem("The authentication token has expired.")

    @jwt.revoked_token_loader
    def _revoked_token(_header, _payload):
        return unauthorized_problem("The authentication token has been revoked.")

    @jwt.needs_fresh_token_loader
    def _needs_fresh(_header, _payload):
        return unauthorized_problem("A fresh authentication token is required.")


def create_app(overrides=None):
    app = Flask(__name__)
    _configure(app, overrides)

    db.init_app(app)
    jwt.init_app(app)
    _register_jwt_handlers()

    app.register_blueprint(health_bp)
    app.register_blueprint(auth_bp)
    app.register_blueprint(articles_bp)

    return app


def init_db(app, retries=30, delay=2.0):
    """Create tables at startup, retrying while the database warms up.

    Idempotent (``create_all`` uses check-first), so it is safe to run from every
    gunicorn worker. Raises once retries are exhausted.
    """
    # Import models so their tables are registered on the metadata.
    from . import models  # noqa: F401

    last_error = None
    for attempt in range(1, retries + 1):
        try:
            with app.app_context():
                db.create_all()
            return
        except Exception as error:  # pragma: no cover - startup race handling
            last_error = error
            app.logger.warning(
                "Database not ready (attempt %s/%s): %s", attempt, retries, error
            )
            time.sleep(delay)
    raise RuntimeError(f"Database unreachable after {retries} attempts: {last_error}")
