# Delete Article Feature

Permanently deletes an article by its ID.

## Endpoint

```
DELETE /api/articles/{id}
```

## Parameters

| Parameter | Type | Location | Description |
|-----------|------|----------|-------------|
| `id` | int | path | The article identifier |

## Response

**Success (204 No Content)**

Empty response body.

**Not Found (404)**

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Not Found",
  "status": 404,
  "detail": "Article with ID 1 was not found."
}
```

## Files

- `IDeleteArticleHandler.cs` - Handler interface
- `DeleteArticleHandler.cs` - Business logic implementation
- `DeleteArticleEndpoint.cs` - HTTP endpoint definition

## Examples

```bash
curl -X DELETE http://localhost:5088/api/articles/1
```

## Tests

- **Unit Tests**: `DeleteArticleHandlerTests.cs`
- **Integration Tests**: `DeleteArticleEndpointTests.cs`