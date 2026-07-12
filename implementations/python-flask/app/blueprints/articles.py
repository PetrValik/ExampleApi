"""Article CRUD, listing and batch creation. All routes require a JWT bearer."""

from math import ceil

from flask import Blueprint, jsonify, request
from flask_jwt_extended import jwt_required

from ..extensions import db
from ..models import Article
from ..problems import conflict_problem, not_found_problem, validation_problem
from ..validation import validate_create, validate_update

articles_bp = Blueprint("articles", __name__, url_prefix="/api")

MAX_PAGE_SIZE = 100
MAX_BATCH = 100


def _parse_positive_int(raw, default, minimum=1):
    if raw is None:
        return default
    try:
        value = int(raw)
    except (TypeError, ValueError):
        return default
    return value if value >= minimum else minimum


def _new_article(data):
    return Article(
        name=data["name"],
        description=data["description"],
        category=data.get("category"),
        price=data["price"],
        currency=data.get("currency"),
        version=1,
    )


@articles_bp.get("/articles")
@jwt_required()
def list_articles():
    name = request.args.get("name")
    category = request.args.get("category")
    page = _parse_positive_int(request.args.get("page"), default=1)
    page_size = _parse_positive_int(request.args.get("pageSize"), default=10)
    if page_size > MAX_PAGE_SIZE:
        page_size = MAX_PAGE_SIZE  # clamp, never reject

    query = Article.query
    if name:
        query = query.filter(Article.name.ilike(f"%{name}%"))
    if category:
        query = query.filter(Article.category == category)

    total_count = query.count()
    total_pages = ceil(total_count / page_size) if total_count else 0
    items = (
        query.order_by(Article.article_id)
        .offset((page - 1) * page_size)
        .limit(page_size)
        .all()
    )

    return jsonify(
        {
            "items": [a.to_response() for a in items],
            "page": page,
            "pageSize": page_size,
            "totalCount": total_count,
            "totalPages": total_pages,
            "hasPrevious": page > 1,
            "hasNext": page < total_pages,
        }
    ), 200


@articles_bp.post("/articles")
@jwt_required()
def create_article():
    body = request.get_json(silent=True)
    if not isinstance(body, dict):
        return validation_problem({"body": ["Request body must be a JSON object."]})

    errors = {}
    data = validate_create(body, errors)
    if errors:
        return validation_problem(errors)

    article = _new_article(data)
    db.session.add(article)
    db.session.commit()

    response = jsonify(article.to_response())
    response.status_code = 201
    response.headers["Location"] = f"/api/articles/{article.article_id}"
    return response


@articles_bp.get("/articles/<int:article_id>")
@jwt_required()
def get_article(article_id):
    article = db.session.get(Article, article_id)
    if article is None:
        return not_found_problem(f"Article with id {article_id} was not found.")
    return jsonify(article.to_response()), 200


@articles_bp.put("/articles/<int:article_id>")
@jwt_required()
def update_article(article_id):
    body = request.get_json(silent=True)
    if not isinstance(body, dict):
        return validation_problem({"body": ["Request body must be a JSON object."]})

    errors = {}
    data = validate_update(body, errors)
    if errors:
        return validation_problem(errors)

    article = db.session.get(Article, article_id)
    if article is None:
        return not_found_problem(f"Article with id {article_id} was not found.")

    if article.version != data["row_version"]:
        return conflict_problem(
            f"Article with id {article_id} was modified by another request. Please retry."
        )

    article.name = data["name"]
    article.description = data["description"]
    article.category = data.get("category")
    article.price = data["price"]
    article.currency = data.get("currency")
    article.version += 1
    db.session.commit()

    return jsonify(article.to_response()), 200


@articles_bp.delete("/articles/<int:article_id>")
@jwt_required()
def delete_article(article_id):
    article = db.session.get(Article, article_id)
    if article is None:
        return not_found_problem(f"Article with id {article_id} was not found.")
    db.session.delete(article)
    db.session.commit()
    return "", 204


@articles_bp.post("/articles-concurrent")
@jwt_required()
def batch_create_articles():
    body = request.get_json(silent=True)
    if not isinstance(body, list):
        return validation_problem({"body": ["Request body must be a JSON array."]})
    if len(body) == 0:
        return validation_problem({"items": ["At least one article is required."]})
    if len(body) > MAX_BATCH:
        return validation_problem(
            {"items": [f"A maximum of {MAX_BATCH} articles can be created at once."]}
        )

    errors = {}
    cleaned = []
    for index, item in enumerate(body):
        if not isinstance(item, dict):
            errors[f"items[{index}]"] = ["Each item must be a JSON object."]
            continue
        item_errors = {}
        data = validate_create(item, item_errors)
        if item_errors:
            for field, messages in item_errors.items():
                errors[f"items[{index}].{field}"] = messages
        else:
            cleaned.append(data)

    if errors:
        return validation_problem(errors)

    articles = [_new_article(data) for data in cleaned]
    db.session.add_all(articles)
    db.session.commit()

    return jsonify([a.to_response() for a in articles]), 201
