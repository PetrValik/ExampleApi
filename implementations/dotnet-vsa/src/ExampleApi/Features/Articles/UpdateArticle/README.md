# Update Article Feature

Updates an existing article with optimistic concurrency control.

## Endpoint

```
PUT /api/articles/{id}
```

## Path Parameters

| Parameter | Type | Description |
|-----------|------|----------|
| `id` | int | Article ID to update |

## Request

```json
{
  "name": "Updated Product Name",
  "description": "Updated description",
  "category": "Electronics",
  "price": 149.99,
  "currency": "EUR",
  "rowVersion": "AAAAAAAAB9E="
}
```

### Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | `true` | Article name (1-200 characters) |
| `description` | string | `false` | Article description (max 2000 characters) |
| `category` | string | `false` | Article category (max 100 characters) |
| `price` | decimal | `true` | Article price (≥ 0) |
| `currency` | string | `false`* | ISO 4217 currency code (required if price > 0) |
| `rowVersion` | string | `true` | Base64-encoded optimistic concurrency token |

*Currency is required only when price is greater than 0.

## Response

**Success (200 OK)**

```json
{
  "articleId": 1,
  "name": "Updated Product Name",
  "description": "Updated description",
  "category": "Electronics",
  "price": 149.99,
  "currency": "EUR",
  "rowVersion": "AAAAAAAAB9F="
}
```

**Validation Error (400 Bad Request)**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "RowVersion": ["RowVersion is required."]
  }
}
```

**Not Found (404 Not Found)**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Article with ID 999 was not found."
}
```

**Concurrency Conflict (409 Conflict)**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Article with ID 1 was modified by another request. Please retry."
}
```

## Optimistic Concurrency Control

### How it works

1. **Client reads article** → receives `rowVersion` token
2. **Client sends update** → includes `rowVersion` from step 1
3. **Server compares** `rowVersion` with current database value
4. **If match** → update succeeds, new `rowVersion` generated
5. **If mismatch** → returns `409 Conflict` (article was modified by someone else)

### Automatic Row Versioning

SQLite trigger automatically updates `RowVersion` on every change:

```sql
CREATE TRIGGER UpdateArticleRowVersion
AFTER UPDATE ON Articles
BEGIN
    UPDATE Articles
    SET RowVersion = randomblob(8)
    WHERE ArticleId = NEW.ArticleId;
END;
```

## Validation Rules

- **Name**: Required, 1-200 characters
- **Description**: Optional, max 2000 characters
- **Category**: Optional, max 100 characters
- **Price**: Required, must be ≥ 0
- **Currency**: Required if price > 0, must be valid ISO 4217 code
- **RowVersion**: Required, must be valid base64 string (8 bytes)

## Files

- `IUpdateArticleHandler.cs` - Handler interface
- `UpdateArticleHandler.cs` - Business logic implementation
- `UpdateArticleEndpoint.cs` - HTTP endpoint definition
- `UpdateArticleRequest.cs` - Request DTO
- `UpdateArticleValidator.cs` - FluentValidation rules
- `ArticleResponse.cs` (shared) - Response DTO

## ExampleApis

### Successful update

```bash
# 1. Get article first
curl http://localhost:5088/api/articles/1

# 2. Update with rowVersion from step 1
curl -X PUT http://localhost:5088/api/articles/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated Name",
    "description": "Updated description",
    "category": "Electronics",
    "price": 149.99,
    "currency": "EUR",
    "rowVersion": "AAAAAAAAB9E="
  }'
```

### Concurrency conflict (409)

```bash
# Simulate two users updating the same article
# User 1 gets article → rowVersion: "AAAAAAAAB9E="
# User 2 gets article → rowVersion: "AAAAAAAAB9E="
# User 1 updates successfully → new rowVersion: "AAAAAAAAB9F="
# User 2 tries to update with old rowVersion → 409 Conflict
```

## Tests

- **Unit Tests**: `UpdateArticleHandlerTests.cs` (6 tests)
- **Validator Tests**: `UpdateArticleValidatorTests.cs` (5 tests)
- **Integration Tests**: `UpdateArticleEndpointTests.cs` (4 tests including concurrency)
