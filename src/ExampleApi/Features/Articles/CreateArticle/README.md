# Create Article Feature

Creates a new article in the system.

## Endpoint

```
POST /api/articles
```

## Request

```json
{
  "name": "Product Name",
  "description": "Product description",
  "category": "Electronics",
  "price": 99.99,
  "currency": "USD"
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

*Currency is required only when price is greater than 0.

## Response

**Success (201 Created)**

```json
{
  "articleId": 1,
  "name": "Product Name",
  "description": "Product description",
  "category": "Electronics",
  "price": 99.99,
  "currency": "USD",
  "rowVersion": "AAAAAAAAB9E="
}
```

**Response Headers**

```
Location: /api/articles/1
```

**Validation Error (400 Bad Request)**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["Name is required."],
    "Currency": ["Currency is required when price is greater than 0."]
  }
}
```

## Validation Rules

- **Name**: Required, 1-200 characters
- **Description**: Optional, max 2000 characters
- **Category**: Optional, max 100 characters
- **Price**: Required, must be ≥ 0
- **Currency**: Required if price > 0, must be valid ISO 4217 code

## Files

- `ICreateArticleHandler.cs` - Handler interface
- `CreateArticleHandler.cs` - Business logic implementation
- `CreateArticleEndpoint.cs` - HTTP endpoint definition
- `ArticleRequest.cs` (shared) - Request DTO
- `ArticleRequestValidator.cs` (shared) - FluentValidation rules
- `ArticleResponse.cs` (shared) - Response DTO

## ExampleApis

### Create paid article

```bash
curl -X POST http://localhost:5088/api/articles \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "High-performance laptop",
    "category": "Electronics",
    "price": 1299.99,
    "currency": "USD"
  }'
```

### Create free article

```bash
curl -X POST http://localhost:5088/api/articles \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Free Sample",
    "description": "Promotional item",
    "price": 0,
    "currency": null
  }'
```

## Tests

- **Unit Tests**: `CreateArticleHandlerTests.cs` (15 tests)
- **Integration Tests**: `CreateArticleEndpointTests.cs` (3 tests)
