# Articles Feature

Complete article (product) management system with CRUD operations and batch processing.

## Overview

The Articles feature provides a RESTful API for managing products/articles with the following capabilities:

- **Create** - Single article creation
- **Read** - Single article retrieval and list with filtering
- **Update** - Article modification with optimistic concurrency control
- **Delete** - Permanent article removal
- **Batch Create** - Concurrent creation of multiple articles

## Features

| Feature | Endpoint | Description |
|---------|----------|-------------|
| [Create Article](CreateArticle/README.md) | `POST /api/articles` | Create a new article |
| [Get Article](GetArticle/README.md) | `GET /api/articles/{id}` | Get article by ID |
| [Get Articles](GetArticles/README.md) | `GET /api/articles` | List articles with filters |
| [Update Article](UpdateArticle/README.md) | `PUT /api/articles/{id}` | Update article with concurrency control |
| [Delete Article](DeleteArticle/README.md) | `DELETE /api/articles/{id}` | Permanently delete an article |
| [Batch Create](BatchCreateArticles/README.md) | `POST /api/articles-concurrent` | Create multiple articles in parallel |

## Data Model

### Article Entity

```csharp
public class Article
{
    public int ArticleId { get; set; }           // Auto-generated primary key
    public required string Name { get; set; }     // Max 64 characters
    public required string Description { get; set; } // Max 2048 characters
    public string? Category { get; set; }         // Max 64 characters
    public decimal Price { get; set; }            // >= 0
    public string? Currency { get; set; }         // ISO 4217 code (USD, EUR, etc.)
    public byte[]? RowVersion { get; set; }       // Optimistic concurrency token
}
```

### Validation Rules

#### Common Rules (ArticleRequest)
- **Name**: Required, 1-64 characters
- **Description**: Required, 1-2048 characters
- **Category**: Optional, max 64 characters
- **Price**: Required, must be ≥ 0
- **Currency**:
  - Required if price > 0
  - Must be valid ISO 4217 currency code
  - Optional/null if price = 0

#### Update-Specific Rules
- **RowVersion**: Required for updates (optimistic concurrency)

#### Batch Create Rules
- Minimum 1 article per request

## Shared Components

Shared components are located in `Features/Articles/Shared/` and organized by type:

### DTOs (`Shared/DTOs/`)
- **ArticleRequest.cs** - Create/Batch create request
- **ArticleResponse.cs** - Standard response format

### Models (`Shared/Models/`)
- **Article.cs** - Entity model

### Validators (`Shared/Validators/`)
- **ArticleRequestValidator.cs** - Validates ArticleRequest

### Mappings (`Shared/Mappings/`)
- **ArticleMappingExtensions.cs** - Entity ↔ DTO mapping

### Feature-Specific Components

Some features have unique requirements:
- **UpdateArticleRequest.cs** (`UpdateArticle/`) - Includes `rowVersion`
- **UpdateArticleValidator.cs** (`UpdateArticle/`) - Validates row version
- **GetArticlesRequest.cs** (`GetArticles/`) - Query parameters
- **BatchCreateArticlesValidator.cs** (`BatchCreateArticles/`) - List validation
- **ArticlesRegistration.cs** (root) - DI registration

## Architecture

Each sub-feature follows **Vertical Slice Architecture**:

```
CreateArticle/
├── ICreateArticleHandler.cs      # Handler interface
├── CreateArticleHandler.cs       # Business logic
├── CreateArticleEndpoint.cs      # HTTP endpoint
└── README.md                     # Feature documentation
```

### Design Principles

- **Interface-based handlers** - Easy to test and mock
- **Scoped lifetime** - Matches DbContext lifetime
- **FluentValidation** - Declarative validation rules
- **Minimal API** - Clean, functional endpoint definitions
- **Read-only queries** - `AsNoTracking()` for better performance

## Database

### Table: Articles

```sql
CREATE TABLE Articles (
    ArticleId INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT NOT NULL,
    Category TEXT,
    Price REAL NOT NULL,
    Currency TEXT,
    RowVersion BLOB NOT NULL
);
```

### Optimistic Concurrency

Automatic row versioning using SQLite trigger:

```sql
CREATE TRIGGER UpdateArticleRowVersion
AFTER UPDATE ON Articles
BEGIN
    UPDATE Articles
    SET RowVersion = randomblob(8)
    WHERE ArticleId = NEW.ArticleId;
END;
```

## Testing

### Test Coverage

| Type | Tests | Coverage |
|------|-------|----------|
| **Unit Tests** | 60 | Handlers + Validators |
| **Integration Tests** | 16 | Full HTTP cycle |

### Unit Tests by Feature

- **CreateArticle**: 3 handler + 10 validator tests
- **GetArticle**: 2 handler tests
- **UpdateArticle**: 4 handler + 13 validator tests
- **DeleteArticle**: 4 handler tests
- **BatchCreateArticles**: 6 handler + 7 validator tests

### Integration Tests by Feature

- **CreateArticle**: 4 tests
- **GetArticles**: 3 tests
- **UpdateArticle**: 2 tests (including concurrency)
- **DeleteArticle**: 3 tests

## Quick Start

### Create an article

```bash
curl -X POST http://localhost:5088/api/articles \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop",
    "description": "Gaming laptop",
    "category": "Electronics",
    "price": 1299.99,
    "currency": "USD"
  }'
```

### Get all articles

```bash
curl http://localhost:5088/api/articles
```

### Get article by ID

```bash
curl http://localhost:5088/api/articles/1
```

### Filter articles

```bash
curl "http://localhost:5088/api/articles?name=laptop&category=Electronics"
```

### Update article

```bash
curl -X PUT http://localhost:5088/api/articles/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated Laptop",
    "description": "Updated description",
    "category": "Electronics",
    "price": 999.99,
    "currency": "USD",
    "rowVersion": "AAAAAAAAB9E="
  }'
```

### Delete article

```bash
curl -X DELETE http://localhost:5088/api/articles/1
```

### Batch create

```bash
curl -X POST http://localhost:5088/api/articles-concurrent \
  -H "Content-Type: application/json" \
  -d '[
    {"name": "Product 1", "description": "Description 1", "price": 10, "currency": "USD"},
    {"name": "Product 2", "description": "Description 2", "price": 20, "currency": "EUR"}
  ]'
```

## Performance Considerations

### Read Operations
- Use `AsNoTracking()` for queries (enabled)
- Filtering happens in database (not in-memory)

### Write Operations
- Single creates: ~10ms per article
- Batch creates: ~2ms per article (5x faster)

### Concurrency
- Optimistic locking prevents lost updates
- Automatic row versioning (no manual tracking)
- 409 Conflict on concurrent modifications

## Error Handling

| Status | When | Response |
|--------|------|----------|
| 200 | Success (create/update) | Article data |
| 204 | Success (delete) | Empty body |
| 400 | Validation failed | Validation errors |
| 404 | Article not found | Error message |
| 409 | Concurrency conflict | Retry message |
| 500 | Server error | Generic error |

## Dependencies

- **Entity Framework Core** - Data access
- **FluentValidation** - Request validation
- **SQLite** - Database provider

## Service Registration

```csharp
// In Program.cs
builder.Services.AddArticlesFeature();

// In ArticlesRegistration.cs
services.AddScoped<ICreateArticleHandler, CreateArticleHandler>();
services.AddScoped<IGetArticleHandler, GetArticleHandler>();
services.AddScoped<IGetArticlesHandler, GetArticlesHandler>();
services.AddScoped<IUpdateArticleHandler, UpdateArticleHandler>();
services.AddScoped<IDeleteArticleHandler, DeleteArticleHandler>();
services.AddScoped<IBatchCreateArticlesHandler, BatchCreateArticlesHandler>();
```