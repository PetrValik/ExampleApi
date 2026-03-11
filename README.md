# Example API

A demonstration RESTful API for article (product) management showcasing modern .NET development practices and clean architecture.

## Overview

This project demonstrates professional API development with .NET 10, featuring:

- **Vertical Slice Architecture** - Features organized by business capability
- **Comprehensive Testing** - 156 tests achieving ~100% code coverage
- **Clean Code** - SOLID principles and separation of concerns
- **Modern .NET Stack** - Latest framework features and best practices
- **Production Patterns** - Validation, error handling, and optimistic concurrency

## Technology Stack

| Category | Technology |
|----------|-----------|
| Framework | .NET 10 |
| Database | SQLite with Entity Framework Core 10.0 |
| Validation | FluentValidation |
| Documentation | OpenAPI + Scalar UI |
| Testing | xUnit + FluentAssertions |
| Architecture | Vertical Slice Architecture |

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- IDE: Visual Studio 2026, VS Code, or Rider

### Running the Application

```bash
# Clone the repository
git clone https://github.com/yourusername/example-api.git
cd example-api

# Run the application
cd src/ExampleApi
dotnet run

# Access the API documentation
# Navigate to http://localhost:5088/scalar/v1
```

### Running Tests

```bash
# Run all tests (156 tests)
dotnet test

# Run only unit tests (128 tests)
dotnet test test/ExampleApi.UnitTests

# Run only integration tests (28 tests)
dotnet test test/ExampleApi.IntegrationTests
```

## Project Structure

```
ExampleApi/
├── src/
│   └── ExampleApi/                    # Main API project
│       ├── Features/                  # Feature slices
│       │   ├── Articles/              # Article management
│       │   │   ├── Shared/            # Common DTOs, models, validators
│       │   │   ├── CreateArticle/     # POST /api/articles
│       │   │   ├── GetArticle/        # GET /api/articles/{id}
│       │   │   ├── GetArticles/       # GET /api/articles (with filters)
│       │   │   ├── UpdateArticle/     # PUT /api/articles/{id}
│       │   │   ├── DeleteArticle/     # DELETE /api/articles/{id}
│       │   │   └── BatchCreateArticles/ # POST /api/articles-concurrent
│       │   └── Health/                # Health check endpoint
│       ├── Common/                    # Shared utilities
│       │   ├── Endpoints/             # Endpoint registration
│       │   ├── Validation/            # Validation helpers
│       │   ├── Exceptions/            # Custom exceptions
│       │   ├── Currency/              # Currency validation
│       │   └── Pagination/            # Pagination models
│       ├── Infrastructure/            # Database and persistence
│       │   └── Database/              # EF Core context and migrations
│       ├── Configuration/             # Service registration
│       └── Program.cs                 # Application entry point
├── test/
│   ├── ExampleApi.UnitTests/          # 128 unit tests
│   └── ExampleApi.IntegrationTests/   # 28 integration tests
└── README.md                          # This file
```

## API Endpoints

### Articles Management

| Method | Endpoint | Description | Status Codes |
|--------|----------|-------------|--------------|
| POST | `/api/articles` | Create a new article | 201, 400 |
| GET | `/api/articles/{id}` | Get article by ID | 200, 404 |
| GET | `/api/articles` | List articles with filters | 200 |
| PUT | `/api/articles/{id}` | Update an article | 200, 400, 404, 409 |
| DELETE | `/api/articles/{id}` | Delete an article | 204, 404 |
| POST | `/api/articles-concurrent` | Batch create articles | 200, 400 |

### Health Check

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | API health status |

### Query Parameters (GET /api/articles)

- `name` - Filter by name (partial match, case-insensitive)
- `category` - Filter by category (exact match)
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 10, max: 100)

## Features

### Validation

- **FluentValidation** - Declarative validation rules
- **Request validation** - Automatic validation before handler execution
- **Business rules** - Currency required for priced items
- **Input constraints** - Length limits, format validation

### Error Handling

- **Global exception handler** - Consistent error responses
- **404 Not Found** - Resource not found exceptions
- **409 Conflict** - Optimistic concurrency violations
- **400 Bad Request** - Validation failures with detailed errors

### Concurrency Control

- **Optimistic locking** - Row versioning with SQLite triggers
- **Conflict detection** - Automatic detection of concurrent updates
- **Retry guidance** - Clients receive current version on conflicts

### Pagination

- **Efficient queries** - Skip/Take with proper metadata
- **Navigation support** - HasNext, HasPrevious indicators
- **Configurable limits** - Protect against excessive data retrieval

## Architecture

### Vertical Slice Architecture

Each feature is organized as a self-contained vertical slice:

```
Feature/
├── README.md              # Feature documentation
├── IHandler.cs           # Handler interface
├── Handler.cs            # Business logic
├── Endpoint.cs           # HTTP endpoint
├── Request.cs            # Request DTO (if specific)
└── Validator.cs          # Validation rules (if specific)
```

**Benefits:**

- **Feature isolation** - Changes don't ripple across the codebase
- **Easy navigation** - All code for a feature in one place
- **Independent testing** - Each slice tested in isolation
- **Team scalability** - Multiple teams work without conflicts
- **Clear boundaries** - Shared vs. feature-specific components

### Dependency Injection

- **Scoped services** - Handlers registered per request
- **Interface-based** - Easy mocking and testing
- **Feature registration** - Each feature registers its own services

### Database

**SQLite** with **Entity Framework Core**:

- Auto-initialization with migrations
- Row versioning via triggers
- In-memory for integration tests
- Support for concurrent operations

## Testing Strategy

### Test Coverage

| Type | Count | Coverage | Database |
|------|-------|----------|----------|
| Unit Tests | 128 | Handlers, Validators, Mappings, Utilities | In-memory |
| Integration Tests | 28 | Full HTTP cycle, All endpoints | SQLite in-memory |
| **Total** | **156** | **~100% code coverage** | - |

### Test Organization

```
test/
├── ExampleApi.UnitTests/
│   ├── Common/                    # Common utilities tests
│   │   ├── Currency/              # Currency validation tests
│   │   ├── Validation/            # Validation filter tests
│   │   └── Pagination/            # Pagination model tests
│   └── Features/
│       └── Articles/              # Article feature tests
│           ├── CreateArticle/     # Create handler + validator tests
│           ├── GetArticle/        # Get handler tests
│           ├── GetArticles/       # List handler tests
│           ├── UpdateArticle/     # Update handler + validator tests
│           ├── DeleteArticle/     # Delete handler tests
│           ├── BatchCreateArticles/ # Batch handler + validator tests
│           └── Shared/
│               └── Mappings/      # Mapping extension tests
└── ExampleApi.IntegrationTests/
    ├── Common/                    # Test infrastructure
    │   ├── TestWebApplicationFactory.cs
    │   └── IntegrationTestBase.cs
    └── Features/
        ├── Articles/              # Article endpoint tests
        │   ├── CreateArticle/
        │   ├── GetArticle/
        │   ├── GetArticles/
        │   ├── UpdateArticle/
        │   ├── DeleteArticle/
        │   └── BatchCreateArticles/
        └── Health/                # Health endpoint tests
```

### Why SQLite for Integration Tests?

- Tests real SQL query translation
- Validates EF Core migrations
- Tests row versioning triggers
- Detects optimistic concurrency
- Fast execution in RAM
- Complete isolation between tests

## Development

### Running Locally

```bash
# Start the API
cd src/ExampleApi
dotnet run

# API available at:
# - http://localhost:5088
# - http://localhost:5088/scalar/v1 (documentation)
# - http://localhost:5088/openapi/v1.json (OpenAPI spec)
```

### Database Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName \
  --project src/ExampleApi/ExampleApi.csproj \
  --output-dir Infrastructure/Database/Migrations

# Apply migrations
dotnet ef database update \
  --project src/ExampleApi/ExampleApi.csproj

# Remove last migration
dotnet ef migrations remove \
  --project src/ExampleApi/ExampleApi.csproj
```

### Adding a New Feature

1. Create feature folder: `src/ExampleApi/Features/YourFeature/`
2. Add handler interface: `IYourFeatureHandler.cs`
3. Add handler implementation: `YourFeatureHandler.cs`
4. Add endpoint: `YourFeatureEndpoint.cs`
5. Add DTOs if needed in `Shared/` or feature folder
6. Add validator if needed
7. Register in DI: Update feature registration
8. Add unit tests: `test/ExampleApi.UnitTests/Features/YourFeature/`
9. Add integration tests: `test/ExampleApi.IntegrationTests/Features/YourFeature/`
10. Add README.md with feature documentation

## Documentation

- **[Main API Documentation](src/ExampleApi/README.md)** - Detailed API guide
- **[Articles Feature](src/ExampleApi/Features/Articles/README.md)** - Article endpoints
- **[Health Check](src/ExampleApi/Features/Health/README.md)** - Health endpoint
- **Individual Features** - Each feature has its own README.md

## Code Quality

### Standards

- **C# 12** language features
- **Nullable reference types** enabled
- **XML documentation** for public APIs
- **Consistent formatting** with .editorconfig
- **SOLID principles** throughout

### Best Practices

- **Validation at the edge** - Validate inputs before business logic
- **Fail fast** - Early validation and error handling
- **Explicit interfaces** - Handler interfaces for testability
- **Separation of concerns** - DTOs, entities, and responses separated
- **Immutability** - Records and readonly properties where appropriate

## API Documentation

When running in Development mode, interactive documentation is available:

- **Scalar UI**: `http://localhost:5088/scalar/v1`
- **OpenAPI Spec**: `http://localhost:5088/openapi/v1.json`

Features:
- Try-it-out functionality
- Request/response examples
- Schema documentation
- Error response examples

## Purpose

This is a demonstration project built for learning and showcasing modern .NET development skills. It implements real-world patterns and practices that would be used in production environments.

**Key Demonstrations:**
- Clean architecture with vertical slices
- Comprehensive unit and integration testing
- Modern C# features and patterns
- RESTful API design best practices
- Database migrations and concurrency handling

## Acknowledgments

This project demonstrates practical implementation of:

- **Vertical Slice Architecture** - Feature-based organization
- **REPR Pattern** - Request-Endpoint-Response structure  
- **Test-Driven Development** - Comprehensive test coverage
- **Domain-Driven Design** - Clean separation of concerns

---

**Framework**: .NET 10  
**Test Coverage**: ~100% (156 tests passing)  
**Purpose**: Demonstration & learning project
