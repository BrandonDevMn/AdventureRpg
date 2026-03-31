# AdventureRpg Server

The AdventureRpg server is a .NET 10 REST API that powers an old-school menu-based RPG. It manages users, characters, activities, and inventory using SQLite via Entity Framework Core.

## Tech Stack

- **Runtime:** .NET 10
- **Framework:** ASP.NET Core Web API (controller-based)
- **Auth:** ASP.NET Core Identity + JWT Bearer tokens
- **Storage:** SQLite via Entity Framework Core
- **API versioning:** Asp.Versioning.Mvc (URL segment: `/v1/...`)
- **API explorer:** Scalar (at `/scalar`)

## Project Structure

```
server/
  Controllers/    — HTTP endpoints
  Data/           — EF Core DbContext and design-time factory
  DTOs/           — Request and response shapes
  Migrations/     — EF Core migrations
  Models/         — Core domain types
  Services/       — Business logic
  Program.cs      — App entry point and DI registration
  appsettings.json
  docs/           — This documentation
```

## Documentation

- [API Reference](api.md) — All endpoints, request/response shapes
- [Auth](auth.md) — Registration, login, and JWT usage
- [Characters](characters.md) — Character classes and stats
- [Fishing](fishing.md) — How the fishing mechanic works
