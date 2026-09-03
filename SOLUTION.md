# Solution Documentation

**Candidate Name:** Rhythem
**Completion Date:** September 4, 2026

---

## Problems Identified

The original implementation works, but has several maintainability and production-readiness issues:

- Controller manually creates `TodoService` with `new`, so dependency injection and unit testing are difficult.
- Service contains both business logic and database code — no separation between the two.
- SQL queries use string interpolation, creating SQL injection risks.
- SQLite connection string is hard-coded rather than read from configuration.
- All operations use `POST` instead of RESTful HTTP methods (`GET`/`PUT`/`DELETE`).
- API exposes the database model directly instead of using request/response DTOs.
- Validation is missing or minimal.
- Exceptions are handled in every controller action and returned as `400 Bad Request`, which can expose internal details and incorrectly classifies genuine server errors as client errors.
- Database calls are synchronous, blocking request threads unnecessarily.
- Tests are placeholder/non-deterministic and depend on the local database instead of testing behavior in isolation.

---

## Architectural Decisions

The solution uses a lightweight layered architecture:

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
EF Core (ORM)
    ↓
SQL Server
```

**Controller** — Handles HTTP concerns, routing, model validation and HTTP responses only.

**Service** — Contains application/business logic and coordinates repository operations.

**Repository** — Contains EF Core data access (DbContext + repository implementation).

**DTOs** — Separate API contracts from the persistence model, so clients can never set server-owned fields (e.g. `Id`, `CreatedAt`) and the database schema can evolve independently of the API shape.

**Dependency Injection** — All dependencies are registered through ASP.NET Core DI, making the code easier to test and maintain.

**Exception Handling** — Exceptions are handled using try/catch blocks in the controller actions, translated into the appropriate HTTP status code.

### Why not Clean Architecture / CQRS / MediatR?

For a small CRUD API with four core operations, these patterns would add unnecessary complexity. The chosen structure provides separation of concerns without over-engineering.

---

## Trade-offs

- **Simplicity over enterprise patterns**: prioritized a clear, four-layer structure (Controller → Service → Repository → EF Core) over CQRS/MediatR, since the API only has four operations and the extra indirection wouldn't pay for itself here.
- **Try/catch over centralized middleware**: kept exception handling in the controller actions rather than introducing global exception-handling middleware, to keep the change set focused; this is called out explicitly under Future Improvements since a single middleware component would remove the repetition across actions.
- **SQL Server + migrations over SQLite/EnsureCreated**: chose SQL Server with EF Core migrations to reflect a realistic production data store and give the project a real schema-evolution story, at the cost of a slightly heavier local setup (SQL Server/LocalDB, `dotnet-ef` tool) compared to a zero-install SQLite file.
- **Unit tests over full integration tests**: focused test effort on deterministic unit tests against mocked repositories/services rather than building out `WebApplicationFactory`-based integration tests, so the suite stays fast and independent of a real database; end-to-end coverage is deferred to Future Improvements.
- **No auth, pagination, or filtering**: deferred these since the assessment scope is basic CRUD; adding them now would have meant guessing at requirements (e.g. per-user ownership model) that weren't specified.

---

## How to Run

### Prerequisites

- .NET SDK (version required by the project) — verify with:
  ```bash
  dotnet --version
  ```
- (Optional) SQL Server or LocalDB if using SQL Server as the data store.

Install required packages (run in the `TodoApi` folder):

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Restore dependencies

From the solution/project directory:

```bash
dotnet restore
```

### Migrations (recommended for SQL Server)

Optional: install the `dotnet-ef` global tool:

```bash
dotnet tool install --global dotnet-ef --version 8.0.0
```

Create a migration:

```bash
dotnet ef migrations add InitialCreate --project TodoApi --startup-project TodoApi
```

Apply the migration:

```bash
dotnet ef database update --project TodoApi --startup-project TodoApi
```

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run
```

Swagger (OpenAPI) is available in Development mode for exploring and testing the endpoints interactively. Learn more at [swagger.io](https://swagger.io/).

### Test

```bash
dotnet test ./TodoApi.Tests
```

---

## API Documentation

| Operation  | Method | Endpoint          | Success         |
|------------|--------|--------------------|-----------------|
| Create     | POST   | `/api/todos`       | 201 Created     |
| Get all    | GET    | `/api/todos`       | 200 OK          |
| Get by ID  | GET    | `/api/todos/{id}`  | 200 OK          |
| Update     | PUT    | `/api/todos/{id}`  | 200 OK          |
| Delete     | DELETE | `/api/todos/{id}`  | 204 No Content  |

Common error responses across all endpoints:
- `400 Bad Request` – invalid input
- `404 Not Found` – TODO does not exist
- `500 Internal Server Error` – unexpected server error

### Create TODO

```
Method: POST
URL: /api/todos
```

Request Body:
```json
{
  "title": "Complete assessment",
  "description": "Refactor the TODO API"
}
```

Response (`201 Created`):
```json
{
  "id": 1,
  "title": "Complete assessment",
  "description": "Refactor the TODO API",
  "isCompleted": false,
  "createdAt": "2026-09-03T14:30:00Z"
}
```

### Get TODO(s)

```
Method: GET
URL: /api/todos          (all TODOs)
URL: /api/todos/{id}     (single TODO)
```

Request: no body.

Response — `GET /api/todos` (`200 OK`, empty array if none exist):
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

Response — `GET /api/todos/1`:
- `200 OK` with the TODO object if found
- `404 Not Found` if it does not exist

### Update TODO

```
Method: PUT
URL: /api/todos/{id}
```

Request Body:
```json
{
  "title": "Complete assessment",
  "description": "Submit the refactored solution",
  "isCompleted": true
}
```

Response:
- `200 OK` with the updated TODO object
- `404 Not Found` if the TODO does not exist

### Delete TODO

```
Method: DELETE
URL: /api/todos/{id}
```

Request: no body.

Response:
- `204 No Content` on success
- `404 Not Found` if the TODO does not exist

---

## Future Improvements

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

### Global exception handling

Replace the per-action try/catch blocks with centralized exception-handling middleware to ensure consistent error responses and logging across every endpoint.

### Integration tests

Add API-level integration tests using `WebApplicationFactory` to validate the complete request pipeline end to end.

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
