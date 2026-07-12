"""SQLAlchemy models.

The optimistic-concurrency token (``row_version`` on the wire) is a plain
integer column that starts at 1 and is incremented on every update. A PUT whose
supplied ``row_version`` differs from the stored value is a stale write and
yields 409. This is the portable approach called out in the contract (the .NET
reference uses PostgreSQL ``xmin``; either satisfies the conformance suite).
"""

from decimal import Decimal

from sqlalchemy import Integer, Numeric, String

from .extensions import db


class Article(db.Model):
    __tablename__ = "articles"

    article_id = db.Column(Integer, primary_key=True, autoincrement=True)
    name = db.Column(String(64), nullable=False)
    description = db.Column(String(2048), nullable=False)
    category = db.Column(String(64), nullable=True)
    # 18 integer digits + 2 fractional — covers the 9999999999999999.99 max.
    price = db.Column(Numeric(20, 2), nullable=False)
    currency = db.Column(String(3), nullable=True)
    # Optimistic-concurrency version, surfaced as ``row_version``.
    version = db.Column(Integer, nullable=False, default=1, server_default="1")

    def to_response(self):
        """Serialize to the exact snake_case wire shape."""
        price = self.price
        if isinstance(price, Decimal):
            price = float(price)
        return {
            "article_id": self.article_id,
            "name": self.name,
            "description": self.description,
            "category": self.category,
            "price": price,
            "currency": self.currency,
            "row_version": self.version,
        }
