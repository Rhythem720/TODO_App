# Solution
## Overview

This solution refactors the existing TODO API into a small, maintainable layered application while keeping the architecture intentionally simple.

The goal was not to introduce unnecessary enterprise patterns, but to address the main production concerns in the original implementation.

## 1. Problems Identified

The original implementation works, but has several maintainability and production-readiness issues:

 - Controller manually creates `TodoService`, so dependency injection and unit testing are difficult.
 - Service contains both business logic and database code.
 - SQL queries use string interpolation, creating SQL injection risks.
 - SQLite connection string is hard-coded.
 - All operations use `POST` instead of RESTful HTTP methods.
 - API exposes the database model directly instead of using request/response DTOs.
 - Validation is missing or minimal.
 - Exceptions are handled in every controller action and returned as `400 Bad Request`, which can expose internal details and incorrectly classify server errors.
 - Database calls are synchronous.
 - Tests are placeholder/non-deterministic and depend on the local database.

## 2. Architectural Decisions

The solution uses a lightweight layered architecture:

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
EFCORE(ORM)
    ↓
SQL Server 
```

### Controller
Handles HTTP concerns, routing, model validation and HTTP responses.

### Service
Contains application/business logic and coordinates repository operations.

### Repository
Contains EF Core data access (DbContext + repository implementation)

### DTOs
Separate API contracts from the persistence model.

### Dependency Injection
All dependencies are registered through ASP.NET Core DI, making the code easier to test and maintain.

### Exception Handling
Unexpected exceptions are handled centrally using middleware and returned as `ProblemDetails`.

### Why not Clean Architecture / CQRS / MediatR?
For a small CRUD API with four core operations, these patterns would add unnecessary complexity. The chosen structure provides separation of concerns without over-engineering.

## 3. How to Run

### Prerequisites

Install the appropriate .NET SDK version required by the project.

Verify the installation:

```bash
dotnet --version
```
(Optional) SQL Server or LocalDB if using SQL Server

Install required packages (run in `TodoApi` folder):

```bash
-dotnet add package Microsoft.EntityFrameworkCore.SqlServer  
-dotnet add package Microsoft.EntityFrameworkCore.Design 
```
### Restore dependencies

From the solution/project directory:

```bash
dotnet restore
```
Migrations (recommended for SQL Server)
optional: install dotnet-ef global tool
```dotnet tool install --global dotnet-ef --version 8.0.0```

create migration

```dotnet ef migrations add InitialCreate --project TodoApi --startup-project TodoApi```

apply migration

```dotnet ef database update --project TodoApi --startup-project TodoApi```

### Build

```bash
dotnet build
```

### Run tests

```bash
dotnet test
```

### Run the API

```bash
dotnet run
```
```
Swagger is available in Development mode.
Swagger (OpenAPI) is an open-source toolset built on the OpenAPI Specification that allows developers to design, document, test, and consume RESTful Web APIs.
To know more - https://swagger.io/
```

## 4. API Documentation

| Operation | Method | Endpoint | Success |
|---|---|---|---|
| Create | POST | `/api/todos` | 201 Created |
| Get all | GET | `/api/todos` | 200 OK |
| Get by ID | GET | `/api/todos/{id}` | 200 OK |
| Update | PUT | `/api/todos/{id}` | 200 OK |
| Delete | DELETE | `/api/todos/{id}` | 204 No Content |

### 4.1 Create TODO

**Request**

```http
POST /api/todos
Content-Type: application/json
```

```json
{
  "title": "Complete assessment",
  "description": "Refactor the TODO API"
}
```

**Response**

```http
201 Created
```

```json
{
  "id": 1,
  "title": "Complete assessment",
  "description": "Refactor the TODO API",
  "isCompleted": false,
  "createdAt": "2026-09-03T14:30:00Z"
}
```


### 4.2 Get all TODOs

```http
GET /api/todos
```

**Response**

```http
200 OK
```

```json
[
  {
    "id": 1,
    "title": "Complete assessment",
    "description": "Refactor the TODO API",
    "isCompleted": false,
    "createdAt": "2026-09-03T14:30:00Z"
  }
]
```

If there are no TODOs, the API returns an empty collection.

---

### 4.3 Get TODO by ID

```http
GET /api/todos/1
```

**Success**

```http
200 OK
```

**When the TODO does not exist**

```http
404 Not Found
```

---

### 4.4 Update TODO

```http
PUT /api/todos/1
Content-Type: application/json
```

```json
{
  "title": "Complete assessment",
  "description": "Submit the refactored solution",
  "isCompleted": true
}
```

**Success**

```http
200 OK
```

**When the TODO does not exist**

```http
404 Not Found
```

---

### 4.5 Delete TODO

```http
DELETE /api/todos/1
```

**Success**

```http
204 No Content
```

**When the TODO does not exist**

```http
404 Not Found
```
Common responses:
- `400 Bad Request` – invalid input
- `404 Not Found` – TODO does not exist
- `500 Internal Server Error` – unexpected server error

##  Testing

Tests cover:

- Unit tests added under `TodoApi.Tests/` using xUnit + Moq.
- Tests mock repository/service to remain deterministic.

Recommended test commands

```dotnet test ./TodoApi.Tests```

### Test principles

Tests are designed to be:
- Deterministic
- Independent
- Readable
- Focused on one behavior
- Easy to maintain

## 5. Future Improvements

If this application were going beyond the scope of the assessment, I would consider the following improvements depending on actual requirements:

### Pagination and filtering

For a growing TODO collection:

```http
GET /api/todos?pageNumber=1&pageSize=20&isCompleted=false
```

### Structured logging

Introduce structured logging using `ILogger` and integrate with a centralized logging/monitoring platform.

### Authentication and authorization

If TODOs become user-specific, add authentication and authorization so users can only access their own TODOs.

### Database migrations

For a production deployment with schema evolution, introduce a proper migration strategy.

### Integration tests

Add API-level integration tests using `WebApplicationFactory` to validate the complete request pipeline.

### CI/CD

Add a CI pipeline that automatically performs:

```text
Restore
   ↓
Build
   ↓
Unit Tests
   ↓
Integration Tests
   ↓
Publish
```

### Observability

Add metrics, distributed tracing, health checks, and application monitoring if the service becomes part of a larger production system.

---

The current implementation intentionally keeps the design simple and appropriate for the application's scope.
