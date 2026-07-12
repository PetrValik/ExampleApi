"""The Article model.

Optimistic concurrency is implemented with an integer ``version`` column that
starts at 1 and is incremented on every successful update. It is surfaced to
clients as ``row_version``. A PUT whose ``row_version`` does not match the
current value performs zero rows and is reported as 409 Conflict.
"""

from django.db import models


class Article(models.Model):
    article_id = models.AutoField(primary_key=True)
    name = models.CharField(max_length=64)
    description = models.CharField(max_length=2048)
    category = models.CharField(max_length=64, null=True, blank=True)
    price = models.DecimalField(max_digits=18, decimal_places=2)
    currency = models.CharField(max_length=3, null=True, blank=True)
    # Optimistic-concurrency token, surfaced as ``row_version``.
    version = models.IntegerField(default=1)

    class Meta:
        db_table = "articles"

    def to_response(self) -> dict:
        """Serialise to the exact snake_case wire shape the contract requires."""
        return {
            "article_id": self.article_id,
            "name": self.name,
            "description": self.description,
            "category": self.category,
            "price": float(self.price),
            "currency": self.currency,
            "row_version": self.version,
        }
