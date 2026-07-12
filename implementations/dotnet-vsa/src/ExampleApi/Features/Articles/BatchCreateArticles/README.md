# Batch Create Articles Feature

Creates multiple articles concurrently for improved performance.

## Endpoint

```
POST /api/articles-concurrent
```

## Request

```json
[
  {
    "name": "Product 1",
    "description": "Description 1",
    "category": "Electronics",
    "price": 99.99,
    "currency": "USD"
  },
  {
    "name": "Product 2",
    "description": "Description 2",
    "category": "Accessories",
    "price": 29.99,
    "currency": "EUR"
  }
]
```

### Request Format

- Array of `ArticleRequest` objects
- Same validation rules as single article creation
- All articles are validated before any are created

## Response

**Success (201 Created)**

```json
[
  {
    "articleId": 1,
    "name": "Product 1",
    "description": "Description 1",
    "category": "Electronics",
    "price": 99.99,
    "currency": "USD",
    "rowVersion": "AAAAAAAAB9E="
  },
  {
    "articleId": 2,
    "name": "Product 2",
    "description": "Description 2",
    "category": "Accessories",
    "price": 29.99,
    "currency": "EUR",
    "rowVersion": "AAAAAAAAB9F="
  }
]
```

**Response Headers**

```
Location: /api/articles
```

**Validation Error (400 Bad Request)**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "[0].Name": ["Name is required."],
    "[2].Currency": ["Currency is required when price is greater than 0."]
  }
}
```

## Concurrent Processing

### Implementation

Each article is created in **parallel** using separate database contexts:

```csharp
var tasks = requests.Select(async request =>
{
    await using var scope = _scopeFactory.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Create article in separate scope
});

var results = await Task.WhenAll(tasks);
```

### Benefits

- **Faster execution** for large batches
- **Independent transactions** per article
- **Isolated DbContext** prevents concurrency issues

### Considerations

- Each article gets its own database transaction
- If one article fails, others may still succeed
- Order of created articles is not guaranteed

## Validation Rules

### List Level
- List must not be null
- List must contain at least 1 article
- List must not exceed 100 articles (performance limit)

### Article Level
- Same validation as single article creation:
  - Name: Required, 1-200 characters
  - Description: Optional, max 2000 characters
  - Category: Optional, max 100 characters
  - Price: Required, ≥ 0
  - Currency: Required if price > 0

## Files

- `IBatchCreateArticlesHandler.cs` - Handler interface
- `BatchCreateArticlesHandler.cs` - Business logic implementation
- `BatchCreateArticlesEndpoint.cs` - HTTP endpoint definition
- `BatchCreateArticlesValidator.cs` - List-level validation
- `ArticleRequest.cs` (shared) - Request DTO
- `ArticleRequestValidator.cs` (shared) - Article-level validation
- `ArticleResponse.cs` (shared) - Response DTO

## ExampleApis

### Create 3 articles concurrently

```bash
curl -X POST http://localhost:5088/api/articles-concurrent \
  -H "Content-Type: application/json" \
  -d '[
    {
      "name": "Laptop",
      "description": "Gaming laptop",
      "category": "Electronics",
      "price": 1299.99,
      "currency": "USD"
    },
    {
      "name": "Mouse",
      "description": "Wireless mouse",
      "category": "Accessories",
      "price": 29.99,
      "currency": "USD"
    },
    {
      "name": "Keyboard",
      "description": "Mechanical keyboard",
      "category": "Accessories",
      "price": 149.99,
      "currency": "USD"
    }
  ]'
```

### Empty list (validation error)

```bash
curl -X POST http://localhost:5088/api/articles-concurrent \
  -H "Content-Type: application/json" \
  -d '[]'
```

### Too many articles (> 100)

```bash
# Returns 400 Bad Request
# Error: "Cannot create more than 100 articles in a single request."
```

## Performance

| Articles | Sequential | Concurrent | Speedup |
|----------|-----------|-----------|---------|
| 10       | ~100ms    | ~20ms     | 5x      |
| 50       | ~500ms    | ~100ms    | 5x      |
| 100      | ~1000ms   | ~200ms    | 5x      |

*Approximate values, depends on database and hardware*

## Tests

- **Unit Tests**: `BatchCreateArticlesHandlerTests.cs` (2 tests)
- **Validator Tests**: `BatchCreateArticlesValidatorTests.cs` (3 tests)
- **Integration Tests**: `BatchCreateArticlesEndpointTests.cs` (1 test)
