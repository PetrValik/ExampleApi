"""Request validators.

Field-level rules (length, price bounds) are enforced by DRF fields so failures
are keyed by field name. The conditional "currency required when price > 0" rule
is enforced in ``validate`` and attributed to the ``currency`` field.
"""

from decimal import Decimal

from rest_framework import serializers

from .currencies import is_supported

_MAX_PRICE = Decimal("9999999999999999.99")


class ArticleRequestSerializer(serializers.Serializer):
    name = serializers.CharField(min_length=1, max_length=64)
    description = serializers.CharField(min_length=1, max_length=2048)
    category = serializers.CharField(
        max_length=64, required=False, allow_null=True, allow_blank=True
    )
    price = serializers.DecimalField(
        max_digits=18,
        decimal_places=2,
        min_value=Decimal("0"),
        max_value=_MAX_PRICE,
        coerce_to_string=False,
    )
    currency = serializers.CharField(
        required=False, allow_null=True, allow_blank=True
    )

    def validate(self, attrs):
        price = attrs.get("price")
        currency = attrs.get("currency")
        # Currency is required & validated only when price > 0; ignored otherwise.
        if price is not None and price > 0:
            if not currency:
                raise serializers.ValidationError(
                    {"currency": ["Currency is required when price is greater than 0."]}
                )
            if len(currency) != 3:
                raise serializers.ValidationError(
                    {"currency": ["Currency must be a valid ISO 4217 code (3 characters)."]}
                )
            if not is_supported(currency):
                raise serializers.ValidationError(
                    {"currency": ["Currency must be a supported currency code."]}
                )
        return attrs


class UpdateArticleRequestSerializer(ArticleRequestSerializer):
    # Required optimistic-concurrency token; must be a non-zero positive integer.
    row_version = serializers.IntegerField(min_value=1)
