# Get Articles Feature

Retrieves a list of articles with optional filtering.

## Endpoint

```
GET /api/articles
```

## Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `name` | string | `false` | Partial name match (case-insensitive) |
| `category` | string | `false` | Exact category match |

## Response

**Success (200 OK)**

```json
[
  {
    "articleId": 1,
    "name": "Laptop",
    "description": "High-performance laptop",
    "category": "Electronics",
    "price": 1299.99,
    "currency": "USD",
    "rowVersion": "AAAAAAAAB9E="
  },
  {
    "articleId": 2,
    "name": "Laptop Stand",
    "description": "Ergonomic laptop stand",
    "category": "Accessories",
    "price": 29.99,
    "currency": "USD",
    "rowVersion": "AAAAAAAAB9F="
  }
]
```

## Pagination

- **Default page size**: 10 items
- **Maximum page size**: 100 items
- **Page numbering**: 1-based (first page is 1)
- **Invalid values**: Automatically clamped to valid range
  - Page < 1 → defaults to 1
  - PageSize < 1 → defaults to 1
  - PageSize > 100 → clamped to 100

## Filtering Behavior

### Name Filter
- **Case-insensitive** partial match
- ExampleApi: `name=laptop` matches "Laptop", "laptop stand", "Gaming Laptop"
- Converted to lowercase: `.ToLower().Contains()`

### Category Filter
- **Exact match** (case-sensitive)
- ExampleApi: `category=Electronics` matches only "Electronics"

### Combined Filters
- Both filters can be used together (AND logic)
- ExampleApi: `?name=laptop&category=Electronics`

## Files

- `IGetArticlesHandler.cs` - Handler interface
- `GetArticlesHandler.cs` - Business logic implementation
- `GetArticlesEndpoint.cs` - HTTP endpoint definition
- `GetArticlesRequest.cs` - Query parameters DTO
- `ArticleResponse.cs` (shared) - Response DTO

## ExampleApis

### Get all articles

```bash
curl http://localhost:5088/api/articles
```

### Filter by name (partial, case-insensitive)

```bash
curl "http://localhost:5088/api/articles?name=laptop"
```

### Filter by category (exact match)

```bash
curl "http://localhost:5088/api/articles?category=Electronics"
```

### Filter by both name and category

```bash
curl "http://localhost:5088/api/articles?name=laptop&category=Electronics"
```

## Performance

- Uses `AsNoTracking()` for read-only queries
- Filters are applied in database query (not in-memory)
- Pagination is performed at database level using `Skip()` and `Take()`
- Total count is calculated efficiently with `CountAsync()`
- Returns empty items array if no matches found (with proper pagination metadata)

## Tests

- **Unit Tests**: `GetArticlesHandlerTests.cs` (4 tests)
- **Integration Tests**: `GetArticlesEndpointTests.cs` (4 tests)
