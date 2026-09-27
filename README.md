# FFXIV API

A simple .NET 10 Web API for FFXIV data with SQL Server integration.

There is *currently* no authentication, as it was built under the assumption I'll just be running it locally for myself. Everything is insecure, blah blah blah

## Prerequisites

- .NET 10 SDK
- SQL Server access (tested with SQL Server 2019+)

### Development Environment

The project uses **User Secrets** to store sensitive configuration like database connection strings locally.

#### Initial Setup

1. Initialize user secrets (already done if cloning this repo):

   ```bash
   dotnet user-secrets init
   ```

2. Set your SQL connection string:

   ```bash
   dotnet user-secrets set "SQL:ConnectionString" "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
   ```

3. View your stored secrets (optional):

   ```bash
   dotnet user-secrets list
   ```

## Running the Application

```bash
cd ffxiv_api
dotnet restore
dotnet run
```

The API will start on `http://localhost:5071` (or check console output for the actual port).

## Running the Tests

```bash
dotnet test
```

The tests don't touch SQL Server. Service tests run against an in-memory SQLite database built from the same EF model.

## Project Structure

```text
ffxiv_api/
├── Controllers/            # HTTP only: bind request DTOs, call a service, map the result to a status code
├── Data/
│   ├── AppDbContext.cs
│   └── Configurations/     # EF mapping, one IEntityTypeConfiguration per table
├── Models/
│   ├── DTOs/               # Request/response shapes (the API contract)
│   ├── Entity/             # Database rows (never bound from or returned to clients)
│   └── Enums/              # Source of truth for ids + labels
├── Services/               # All database access and business rules
└── Program.cs
ffxiv_api.Tests/            # xUnit tests
```

### Conventions

- **Services own the `AppDbContext`.** Controllers never query the database directly.
- **Entities never cross the HTTP boundary.** Requests bind to `*Request` DTOs (only writable fields). Responses are built with `*Response.FromEntity(...)`.
- **Expected failures** (validation, not found, conflicts) are returned from services as a `ServiceError` and become `400`/`404`/`409` with a `{ "error": "..." }` body.
- **Unexpected failures** just throw. The global exception handler logs them and returns a ProblemDetails `500`.
- **The schema is managed outside EF** (no migrations). If you change a table, update the matching class in `Data/Configurations/` to mirror it.
