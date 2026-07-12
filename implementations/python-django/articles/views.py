"""HTTP endpoints for the Example API.

Plain DRF ``APIView`` classes are used (rather than a router/ViewSet) so status
codes, the ``Location`` header, pagination clamping and the batch semantics all
match the wire contract exactly.
"""

import math

from django.db.models import F
from rest_framework import status
from rest_framework.exceptions import NotFound, ValidationError
from rest_framework.permissions import AllowAny, IsAuthenticated
from rest_framework.response import Response
from rest_framework.views import APIView

from .auth import issue_token
from .exceptions import ConflictException
from .models import Article
from .serializers import ArticleRequestSerializer, UpdateArticleRequestSerializer

DEFAULT_PAGE_SIZE = 10
MAX_PAGE_SIZE = 100
MAX_BATCH_SIZE = 100

DEMO_USERNAME = "admin"
DEMO_PASSWORD = "admin"


def _parse_int(value, default):
    try:
        return int(value)
    except (TypeError, ValueError):
        return default


# --- Health ---------------------------------------------------------------

class HealthView(APIView):
    authentication_classes: list = []
    permission_classes = [AllowAny]

    def get(self, request):
        return Response({"status": "healthy"})


# --- Auth -----------------------------------------------------------------

class TokenView(APIView):
    authentication_classes: list = []
    permission_classes = [AllowAny]

    def post(self, request):
        data = request.data if isinstance(request.data, dict) else {}
        username = data.get("username")
        password = data.get("password")

        if username != DEMO_USERNAME or password != DEMO_PASSWORD:
            return Response(
                {"detail": "Invalid credentials."},
                status=status.HTTP_401_UNAUTHORIZED,
            )

        token, expires_at = issue_token(username)
        return Response(
            {
                "token": token,
                "expiresAt": expires_at.strftime("%Y-%m-%dT%H:%M:%SZ"),
            }
        )


# --- Articles: list + create ----------------------------------------------

class ArticleListCreateView(APIView):
    permission_classes = [IsAuthenticated]

    def get(self, request):
        name = request.query_params.get("name")
        category = request.query_params.get("category")
        page = max(1, _parse_int(request.query_params.get("page"), 1))
        page_size = _parse_int(request.query_params.get("pageSize"), DEFAULT_PAGE_SIZE)
        page_size = min(max(page_size, 1), MAX_PAGE_SIZE)  # clamp to 1..100

        queryset = Article.objects.all()
        if name:
            queryset = queryset.filter(name__icontains=name)
        if category:
            queryset = queryset.filter(category=category)

        total_count = queryset.count()
        total_pages = math.ceil(total_count / page_size) if total_count else 0

        offset = (page - 1) * page_size
        items = [
            article.to_response()
            for article in queryset.order_by("article_id")[offset : offset + page_size]
        ]

        return Response(
            {
                "items": items,
                "page": page,
                "pageSize": page_size,
                "totalCount": total_count,
                "totalPages": total_pages,
                "hasPrevious": page > 1,
                "hasNext": page < total_pages,
            }
        )

    def post(self, request):
        serializer = ArticleRequestSerializer(data=request.data)
        serializer.is_valid(raise_exception=True)
        data = serializer.validated_data

        article = Article.objects.create(
            name=data["name"],
            description=data["description"],
            category=data.get("category"),
            price=data["price"],
            currency=data.get("currency"),
        )

        response = Response(article.to_response(), status=status.HTTP_201_CREATED)
        response["Location"] = f"/api/articles/{article.article_id}"
        return response


# --- Articles: batch create -----------------------------------------------

class ArticleBatchCreateView(APIView):
    permission_classes = [IsAuthenticated]

    def post(self, request):
        payload = request.data
        if not isinstance(payload, list):
            raise ValidationError({"body": ["Expected a JSON array of articles."]})
        if len(payload) == 0:
            raise ValidationError({"body": ["The batch must contain at least one item."]})
        if len(payload) > MAX_BATCH_SIZE:
            raise ValidationError(
                {"body": [f"The batch must not exceed {MAX_BATCH_SIZE} items."]}
            )

        serializer = ArticleRequestSerializer(data=payload, many=True)
        serializer.is_valid(raise_exception=True)

        created = [
            Article.objects.create(
                name=item["name"],
                description=item["description"],
                category=item.get("category"),
                price=item["price"],
                currency=item.get("currency"),
            ).to_response()
            for item in serializer.validated_data
        ]

        return Response(created, status=status.HTTP_201_CREATED)


# --- Articles: get / update / delete --------------------------------------

class ArticleDetailView(APIView):
    permission_classes = [IsAuthenticated]

    def _get_or_404(self, article_id: int) -> Article:
        try:
            return Article.objects.get(pk=article_id)
        except Article.DoesNotExist as exc:
            raise NotFound(f"Article with ID {article_id} was not found.") from exc

    def get(self, request, article_id: int):
        article = self._get_or_404(article_id)
        return Response(article.to_response())

    def put(self, request, article_id: int):
        # Validate first (matches the canonical reference ordering).
        serializer = UpdateArticleRequestSerializer(data=request.data)
        serializer.is_valid(raise_exception=True)
        data = serializer.validated_data

        article = self._get_or_404(article_id)
        expected_version = data["row_version"]

        # Optimistic concurrency: conditional update on the current version.
        updated_rows = (
            Article.objects.filter(pk=article_id, version=expected_version).update(
                name=data["name"],
                description=data["description"],
                category=data.get("category"),
                price=data["price"],
                currency=data.get("currency"),
                version=F("version") + 1,
            )
        )
        if updated_rows == 0:
            # The row exists (fetched above) but the version was stale.
            raise ConflictException(
                f"Article with ID {article_id} was modified by another request."
            )

        article.refresh_from_db()
        return Response(article.to_response())

    def delete(self, request, article_id: int):
        article = self._get_or_404(article_id)
        article.delete()
        return Response(status=status.HTTP_204_NO_CONTENT)
