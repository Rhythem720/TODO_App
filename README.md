# TODO API

A refactored ASP.NET Core CRUD API for managing todo items — RESTful endpoints, layered
architecture (Controller → Service → Repository → EF Core), and a matching xUnit test suite.

See [`SOLUTION.md`](./SOLUTION.md) for the problems identified in the original implementation,
the architectural decisions and trade-offs behind this refactor, and future improvements.

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core (SQL Server provider) with code-first migrations
- Swagger / OpenAPI (Swashbuckle)
- xUnit + Moq (unit tests) + `Microsoft.AspNetCore.Mvc.Testing` (integration tests, run against an
  in-memory SQLite database so they don't require a real SQL Server instance — see `SOLUTION.md`
  for that trade-off)

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server or [SQL Server LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)
  (the default connection string in `appsettings.json` targets `(localdb)\mssqllocaldb`;
  update it — or the `TodoDb` connection string — to point at your own instance if needed)
- (Optional) [SSMS](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms)
  or Azure Data Studio to inspect the database

## Quick Start

The project already references the packages below in `TodoApi.csproj`, so a plain `dotnet restore`
pulls them in — you only need the explicit `dotnet add package` commands if you're wiring EF Core
into a project that doesn't have them yet:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

- `Microsoft.EntityFrameworkCore.SqlServer` — the SQL Server provider EF Core uses at runtime.
- `Microsoft.EntityFrameworkCore.Design` — design-time tooling required for `dotnet ef migrations add`
  and `dotnet ef database update` to work.

```bash
# Restore dependencies
dotnet restore

# Install the EF Core CLI tool once, if you don't already have it
dotnet tool install --global dotnet-ef --version 8.0.0

# Create the initial migration (first time only)
dotnet ef migrations add InitialCreate --project TodoApi --startup-project TodoApi

# Apply it to your SQL Server / LocalDB instance
dotnet ef database update --project TodoApi --startup-project TodoApi

# Build
dotnet build

# Run the API (from the TodoApi/ folder, or pass --project)
dotnet run --project TodoApi

# Run the tests
dotnet test
```

The API listens on the URL shown in the console output (see `TodoApi/Properties/launchSettings.json`).
With the app running in Development mode, open `/swagger` in a browser to explore and try the
endpoints interactively.

## API Endpoints

| Method | Route              | Description                  |
|--------|---------------------|-------------------------------|
| POST   | `/api/todos`        | Create a new todo item        |
| GET    | `/api/todos`        | Get all todo items            |
| GET    | `/api/todos/{id}`   | Get a single todo item by id  |
| PUT    | `/api/todos/{id}`   | Update an existing todo item  |
| DELETE | `/api/todos/{id}`   | Delete a todo item            |

Full request/response examples and status codes are documented in `SOLUTION.md`.

## Project Structure

```
TodoApi/
├── Controllers/    # Thin HTTP controllers - routing and status codes only
├── Services/       # Business logic, DTO<->entity mapping
├── Repositories/   # EF Core data access
├── Data/           # DbContext
├── Dtos/           # Request/response contracts (kept separate from the EF entity)
├── Models/         # EF Core entity
├── Exceptions/      # Domain exceptions (e.g. NotFoundException)
├── Middleware/      # Centralized exception handling -> problem+json responses
└── Program.cs       # Composition root (DI, pipeline)

TodoApi.Tests/
├── Services/        # Unit tests for the service layer (mocked repository)
├── Controllers/      # Unit tests for the controller layer (mocked service)
└── Integration/      # Full HTTP pipeline tests against an in-memory SQLite database
```

## Testing

```bash
dotnet test
```

The suite covers both positive and negative cases per endpoint (found/not-found, valid/invalid
input), plus a regression test proving the original SQL-injection vector is closed.
