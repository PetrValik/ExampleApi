"""Shared extension singletons.

Kept in their own module so blueprints and the app factory can import the same
instances without creating circular imports.
"""

from flask_jwt_extended import JWTManager
from flask_sqlalchemy import SQLAlchemy

# Single SQLAlchemy handle bound to the app inside ``create_app``.
db = SQLAlchemy()

# JWT manager (HS256). Bound to the app inside ``create_app``.
jwt = JWTManager()
