# UnitTests

Unit tests for the `server` project. Services and controllers are tested in isolation using mocks and an EF Core in-memory database. No running server or real database is required.

## Tech Stack

- **Framework:** xUnit
- **Mocking:** Moq
- **Database:** `Microsoft.EntityFrameworkCore.InMemory`
- **Coverage:** coverlet — target ≥ 80% branch coverage

## What Is Tested

| Area | Notes |
|---|---|
| `CharacterService` | All four classes, user scoping, CRUD, delete with items |
| `InventoryService` | Add, get, multi-item, per-character isolation |
| `FishingService` | Miss, catch, all 4 rarities, agility bonus, junk vs fish type, inventory side effects |
| `TokenService` | Access token claims, refresh token generation, revocation |
| `CharacterController` | All endpoints, 404 paths |
| `FishingController` | Cast found/not found |
| `InventoryController` | Get found/not found |
| `AuthController` | Register, login, refresh, logout, delete account |

## Project Structure

```
server-tests/UnitTests/
  Helpers/
    DbContextFactory.cs        — creates EF Core in-memory DbContext
    ControlledRandom.cs        — deterministic Random for fishing dice tests
    ControllerTestHelpers.cs   — sets a fake authenticated user on a controller
  Services/
    CharacterServiceTests.cs
    InventoryServiceTests.cs
    FishingServiceTests.cs
    TokenServiceTests.cs
  Controllers/
    CharacterControllerTests.cs
    FishingControllerTests.cs
    InventoryControllerTests.cs
    AuthControllerTests.cs
  coverage.runsettings         — excludes migrations, Program, and design-time factory
  docs/                        — this documentation
```

## Running Tests

```bash
dotnet test
```

## Checking Branch Coverage

From `server-tests/`:

```bash
./coverage-branch.sh
```

Outputs a percentage. Target: **≥ 80%**
