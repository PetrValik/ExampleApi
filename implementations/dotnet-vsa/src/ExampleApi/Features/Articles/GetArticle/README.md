# Get Article Feature

Retrieves a single article by its unique identifier.

## Endpoint

```
GET /api/articles/{articleId}
```

## Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `articleId` | int | Unique article identifier |

## Response

**Success (200 OK)**

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

**Not Found (404 Not Found)**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Article with ID 999 not found."
}
```

## Behavior

- Uses `AsNoTracking()` for read-only queries (better performance)
- Throws `NotFoundException` if article doesn't exist
- Returns all article fields including optimistic concurrency token (`rowVersion`)

## Files

- `IGetArticleHandler.cs` - Handler interface
- `GetArticleHandler.cs` - Business logic implementation
- `GetArticleEndpoint.cs` - HTTP endpoint definition
- `ArticleResponse.cs` (shared) - Response DTO

## ExampleApis

### Get existing article

```bash
curl http://localhost:5088/api/articles/1
```

### Get non-existing article (404)

```bash
curl http://localhost:5088/api/articles/999
```

## Tests

- **Unit Tests**: `GetArticleHandlerTests.cs` (2 tests)
- **Integration Tests**: `GetArticleEndpointTests.cs` (2 tests)
