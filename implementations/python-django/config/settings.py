"""Django settings for the Example API (Django + DRF implementation).

Configuration is environment-driven so the same image runs locally and in
Docker Compose. Database credentials, the JWT signing secret, issuer, audience
and token lifetime all come from environment variables (with sensible demo
defaults).
"""

import os
from pathlib import Path

BASE_DIR = Path(__file__).resolve().parent.parent


def _env_bool(name: str, default: bool = False) -> bool:
    value = os.environ.get(name)
    if value is None:
        return default
    return value.strip().lower() in {"1", "true", "yes", "on"}


# --- Core Django -----------------------------------------------------------

SECRET_KEY = os.environ.get(
    "DJANGO_SECRET_KEY",
    "django-insecure-demo-key-change-me-0123456789abcdef",
)

DEBUG = _env_bool("DJANGO_DEBUG", False)

ALLOWED_HOSTS = os.environ.get("DJANGO_ALLOWED_HOSTS", "*").split(",")

INSTALLED_APPS = [
    "rest_framework",
    "articles",
]

MIDDLEWARE = [
    "django.middleware.security.SecurityMiddleware",
    "django.middleware.common.CommonMiddleware",
]

ROOT_URLCONF = "config.urls"

TEMPLATES = [
    {
        "BACKEND": "django.template.backends.django.DjangoTemplates",
        "DIRS": [],
        "APP_DIRS": False,
        "OPTIONS": {"context_processors": []},
    },
]

WSGI_APPLICATION = "config.wsgi.application"
ASGI_APPLICATION = "config.asgi.application"

DEFAULT_AUTO_FIELD = "django.db.models.BigAutoField"


# --- Database (PostgreSQL, env-driven) -------------------------------------

DATABASES = {
    "default": {
        "ENGINE": "django.db.backends.postgresql",
        "NAME": os.environ.get("POSTGRES_DB", "exampleapi"),
        "USER": os.environ.get("POSTGRES_USER", "postgres"),
        "PASSWORD": os.environ.get("POSTGRES_PASSWORD", "postgres"),
        "HOST": os.environ.get("POSTGRES_HOST", "localhost"),
        "PORT": os.environ.get("POSTGRES_PORT", "5432"),
    }
}


# --- Internationalisation --------------------------------------------------

LANGUAGE_CODE = "en-us"
TIME_ZONE = "UTC"
USE_I18N = False
USE_TZ = True


# --- Django REST Framework -------------------------------------------------

REST_FRAMEWORK = {
    # Custom JWT bearer authentication (demo user, no user table required).
    "DEFAULT_AUTHENTICATION_CLASSES": [
        "articles.auth.JWTAuthentication",
    ],
    # Every endpoint requires a bearer token unless it opts out (health, token).
    "DEFAULT_PERMISSION_CLASSES": [
        "rest_framework.permissions.IsAuthenticated",
    ],
    # Pure JSON API — no browsable API, no template/static dependencies.
    "DEFAULT_RENDERER_CLASSES": [
        "rest_framework.renderers.JSONRenderer",
    ],
    "DEFAULT_PARSER_CLASSES": [
        "rest_framework.parsers.JSONParser",
    ],
    # No user model — request.user is None when unauthenticated.
    "UNAUTHENTICATED_USER": None,
    # Normalise every framework error to RFC 7807 application/problem+json.
    "EXCEPTION_HANDLER": "articles.exceptions.problem_exception_handler",
}


# --- JWT (HS256) -----------------------------------------------------------

JWT_SECRET = os.environ.get(
    "JWT_SECRET",
    "CHANGE-THIS-DEMO-SECRET-AT-LEAST-32-CHARACTERS-LONG",
)
JWT_ISSUER = os.environ.get("JWT_ISSUER", "ExampleApi")
JWT_AUDIENCE = os.environ.get("JWT_AUDIENCE", "ExampleApiClient")
JWT_EXPIRATION_MINUTES = int(os.environ.get("JWT_EXPIRATION_MINUTES", "60"))
