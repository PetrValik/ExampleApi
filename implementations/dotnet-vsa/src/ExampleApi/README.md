# Example API

RESTful API for article (product) management built with **.NET 10** and **Vertical Slice Architecture**.

## Quick Start

```bash
# Run the application
dotnet run

# Access interactive API documentation
open http://localhost:5088/scalar/v1

# Run tests
dotnet test ..\ExampleApi.UnitTests\ExampleApi.UnitTests.csproj
dotnet test ..\ExampleApi.IntegrationTests\ExampleApi.IntegrationTests.csproj
```

## Running the Application

### Using CLI

```bash
dotnet run
```

### Using Visual Studio

Press `F5` or click "Start Debugging"

## Available URLs

When running the application in **Development** mode:

```
http://localhost:5088                - Main API
http://localhost:5088/openapi/v1.json - OpenAPI specification
http://localhost:5088/scalar/v1       - Scalar UI (interactive documentation)
```

## Technologies

- **.NET 10** - Framework
- **SQLite** - Database
- **Entity Framework Core 10.0** - ORM
- **FluentValidation** - Validation
- **OpenAPI** - API documentation
- **Scalar** - API UI documentation

## API Endpoints

Full API documentation: **[Features/Articles/README.md](Features/Articles/README.md)**

### Articles

| Endpoint | Method | Description | Documentation |
|----------|--------|-------------|---------------|
| `/api/articles` | POST | Create a new article | [→ docs](Features/Articles/CreateArticle/README.md) |
| `/api/articles/{id}` | GET | Get article by ID | [→ docs](Features/Articles/GetArticle/README.md) |
| `/api/articles` | GET | Get articles with filters | [→ docs](Features/Articles/GetArticles/README.md) |
| `/api/articles/{id}` | PUT | Update an article | [→ docs](Features/Articles/UpdateArticle/README.md) |
| `/api/articles/{id}` | DELETE | Delete an article | [→ docs](Features/Articles/DeleteArticle/README.md) |
| `/api/articles-concurrent` | POST | Batch create articles | [→ docs](Features/Articles/BatchCreateArticles/README.md) |

### Health

- `GET /health` - Health check endpoint ([→ docs](Features/Health/README.md))

## Tests

### Run All Tests

```bash
# Unit tests (128 tests)
dotnet test ..\ExampleApi.UnitTests\ExampleApi.UnitTests.csproj

# Integration tests (28 tests)
dotnet test ..\ExampleApi.IntegrationTests\ExampleApi.IntegrationTests.csproj

# All tests (156 tests)
dotnet test
```

### Test Strategy

| Type | Count | What it tests | Database |
|------|-------|---------------|----------|
| **Unit** | 128 | Handlers, Validators, Mappings, Utilities | In-memory |
| **Integration** | 28 | Full HTTP cycle, All endpoints | SQLite in-memory |
| **Total** | **156** | ~100% code coverage | - |

### Why SQLite for Integration Tests?

- Tests **real SQL** queries and constraints
- Tests **EF Core migrations** and actual DB behavior
- Tests **row versioning** with automatic triggers
- Tests **optimistic concurrency** detection
- Fast execution (runs in RAM)
- Isolated state per test

## Database

**Type:** SQLite (file-based)  
**File:** `ExampleApi.db` (auto-created on first run)

The database is automatically initialized using EF Core migrations.

### Entity Framework Commands

```bash
# Create new migration
dotnet ef migrations add MigrationName \
  --project ExampleApi.csproj \
  --output-dir Infrastructure/Database/Migrations

# Apply migrations
dotnet ef database update --project ExampleApi.csproj

# Remove last migration
dotnet ef migrations remove --project ExampleApi.csproj
```

### Features

- **Auto-initialization** - Database created on first app run
- **Optimistic concurrency** - Row versioning with SQLite triggers
- **Migrations** - Version-controlled schema changes
- **In-memory for tests** - Fast, isolated test databases

## Architecture

The project uses **Vertical Slice Architecture** with organization by features.

### Project Structure

```
Features/
  Articles/                    # Articles feature (CRUD)
    Shared/                    # DTOs, models, mappings, validators
    CreateArticle/             # POST /api/articles
    GetArticle/                # GET /api/articles/{id}
    GetArticles/               # GET /api/articles
    UpdateArticle/             # PUT /api/articles/{id}
    DeleteArticle/             # DELETE /api/articles/{id}
    BatchCreateArticles/       # POST /api/articles-concurrent
    ArticlesRegistration.cs    # DI registration
  Health/                      # GET /health
```

### Vertical Slice Pattern

Each feature slice contains:
- **README.md** - Feature documentation
- **Interface** - Handler interface for DI and testing
- **Handler** - Business logic implementation
- **Endpoint** - HTTP endpoint definition
- **DTOs** - Request/Response objects (in Shared/)
- **Validator** - FluentValidation rules (in Shared/ or feature-specific)

### Shared vs. Feature-Specific Components

**Shared/** (used by multiple features):
- Common DTOs (`ArticleRequest`, `ArticleResponse`)
- Entity model (`Article`)
- Mapping extensions
- Common validators (`ArticleRequestValidator`)

**Feature-Specific** (unique to one feature):
- `UpdateArticleRequest` - Includes `rowVersion` for concurrency
- `UpdateArticleValidator` - Validates row version
- `BatchCreateArticlesValidator` - Validates list of articles

**Benefits:**
- Feature independence - each feature is self-contained
- Easy to understand - all code for a feature in one place
- Easy to test - isolated unit and integration tests
- Easy to extend - add new features without touching existing ones
- Clear separation - Shared vs. feature-specific components

For detailed feature documentation, see **[Features/Articles/README.md](Features/Articles/README.md)**