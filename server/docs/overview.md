# AdventureRpg Server

The AdventureRpg server is a .NET 10 REST API that powers an old-school menu-based RPG. It manages characters, activities, and inventory using in-memory storage.

## Tech Stack

- **Runtime:** .NET 10
- **Framework:** ASP.NET Core Web API (controller-based)
- **Storage:** In-memory (singleton services)

## Project Structure

```
server/
  AdventureRpg/
    Controllers/    — HTTP endpoints
    DTOs/           — Request and response shapes
    Models/         — Core domain types
    Services/       — Business logic
    Program.cs      — App entry point and DI registration
  docs/             — This documentation
```

## Documentation

- [API Reference](api.md) — All endpoints, request/response shapes
- [Characters](characters.md) — Character classes and stats
- [Fishing](fishing.md) — How the fishing mechanic works
