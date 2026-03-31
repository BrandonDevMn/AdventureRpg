# Maintainer Guide

How this project is structured, the patterns used in each layer, and the conventions to follow when adding new features.

## Project Layout

```
AdventureRpg/
  server/              — .NET 10 REST API (game backend)
  server-tests/
    UnitTests/         — xUnit unit tests (no server required)
    ApiTests/          — xUnit end-to-end tests (live server required)
  client-cli/          — .NET 10 interactive terminal client
  client-cli-tests/    — xUnit unit tests for the client (no server required)
  docs/                — this folder
```

Each project with a `.csproj` has its own `docs/` folder. Update the relevant docs whenever you change behaviour.

---

## Server Patterns

### Adding an Endpoint

1. **Model** — add a domain type to `Models/` if the concept is new (e.g. `Quest.cs`).
2. **DTO** — add request/response types to `DTOs/`. Keep them flat; don't reuse domain models directly.
3. **Service** — add an interface + implementation to `Services/`. Register as `Scoped` in `Program.cs`.
4. **Controller** — add a controller to `Controllers/`. Inherit from `ControllerBase`, apply `[Authorize]` and `[ApiVersion("1.0")]` as appropriate.
5. **Migration** — if the schema changed, run `dotnet ef migrations add <Name>` from the `server/` folder.

### Authentication & Authorization

- Every endpoint that belongs to a specific user must read `User.FindFirstValue(ClaimTypes.NameIdentifier)` to get the `userId` and pass it to the service. Never trust a userId from the request body.
- Access tokens expire after **60 minutes**. Refresh tokens last until logout.
- `TokenService` handles all token creation and revocation. Do not manipulate `RefreshToken` rows directly from controllers.

### Service Layer Rules

- Services accept a `userId` parameter on every method that touches user-owned data.
- Services must not call other services. If shared logic is needed, extract it to a helper or move it up to the controller.
- `FishingService` receives `Random` via its constructor (optional). Pass a `ControlledRandom` subclass in tests for deterministic results.
- Registrations in `Program.cs`: services are `Scoped`, `TokenService` is `Scoped`, `Random` is `Singleton`.

### EF Core Conventions

- `AppDbContext` extends `IdentityDbContext<IdentityUser>`.
- Enum columns are stored as strings (`HasConversion<string>()`).
- Unique indexes are declared in `OnModelCreating` (e.g. `RefreshToken.Token`).
- Run migrations on startup via `db.Database.MigrateAsync()` in `Program.cs`.
- The `AppDbContextFactory` is for design-time tooling only; do not use it at runtime.

### API Versioning

All routes use the URL-segment scheme: `/v{version}/...`. Controllers declare `[Route("v{version:apiVersion}/[controller]")]` and `[ApiVersion("1.0")]`. When adding a breaking change, increment the version and keep the old route working until clients are updated.

### Coverage Target

Server unit tests must maintain **≥ 80% branch coverage**. Check with:

```bash
cd server-tests && ./coverage-branch.sh
```

Migrations, `Program.cs`, and `AppDbContextFactory` are excluded from the calculation via `coverage.runsettings`.

---

## Client CLI Patterns

### Screen Conventions

- Each screen is a class in `Screens/` with a single public `async Task<T> ShowAsync(...)` method.
- Screens call `ApiClient` methods directly; they do not hold state between calls.
- All Spectre.Console rendering happens inside the screen class. `Program.cs` only handles navigation.
- Confirmation prompts for destructive actions default to `false`.

### Navigation

`Program.cs` owns the navigation loop. The pattern:

```
outer while(true)
  → if not logged in, show WelcomeScreen
  → inner while(IsLoggedIn), show CharacterSelectScreen
    → inner while(true), show GameScreen
      → BackToCharacters → break inner game loop
      → Logout / DeletedAccount → goto NextLogin (breaks both inner loops)
```

Session restore runs before the outer loop. If a saved refresh token is found, `RestoreSessionAsync` is called; on success the welcome screen is skipped.

### ApiClient Rules

- `ApiClient` is the only class that talks to the server.
- All protected calls go through `SendAuthorized`, which retries once after a 401 by calling `TryRefreshAsync`.
- On login/register/refresh, tokens are stored in private fields **and** persisted to disk via `SessionStore`.
- On logout/delete-account, `ClearTokens` removes in-memory tokens and deletes the session file.
- The internal `ApiClient(HttpClient, SessionStore?)` constructor is for tests only (`InternalsVisibleTo` the test project).

### Session Persistence

The refresh token is saved to `~/.adventurerpg/session` (plain text). On startup `Program.cs` calls `sessionStore.Load()` and, if a token is found, attempts `api.RestoreSessionAsync(token)`. This mirrors patterns used by tools like the GitHub CLI (`~/.config/gh/`) and AWS CLI (`~/.aws/credentials`).

`SessionStore` swallows all I/O exceptions — session persistence is best-effort and must never crash the application.

### Adding a New Screen

1. Create `Screens/YourScreen.cs` with a `ShowAsync` method returning a meaningful enum or model.
2. Instantiate it in `Program.cs` at the top alongside the other screens.
3. Wire the return value into the navigation loop.
4. Add the screen to `client-cli/docs/screens.md`.
5. Screens are excluded from coverage — do not add unit tests for them.

---

## Test Patterns

### Server Unit Tests

- Use `DbContextFactory.Create()` to get a fresh EF Core in-memory `AppDbContext` per test.
- Use `ControllerTestHelpers.SetUser(controller, userId)` to inject a fake authenticated user into a controller.
- Use `ControlledRandom` to drive deterministic outcomes in `FishingService` tests.
- Mock `ITokenService` with Moq; do not mock `CharacterService` or `InventoryService` — use the real service with an in-memory DB.

### Client Unit Tests

- Use `MockHttpHandler` to queue responses: `handler.Enqueue(HttpStatusCode.OK, new { ... })`.
- Use `ApiClientFactory.Create(handler)` to build an `ApiClient` wired to the mock.
- After the test, assert `handler.Requests` to verify the correct endpoints were called.
- Screens are excluded from coverage (they depend on `AnsiConsole` static API).

### End-to-End API Tests

- Require a live server at the URL configured in `server-tests/ApiTests/appsettings.json`.
- Each run uses a unique `test-{guid}@adventure.com` email so tests are safe to repeat.
- The single `FullGameFlow` fact covers the entire user journey from register to account deletion.

---

## Adding a New Game Feature (full-stack example)

Suppose you want to add a **combat** mechanic:

1. `server/Models/Enemy.cs` — domain model
2. `server/DTOs/CombatRequests.cs` — request/response DTOs
3. `server/Services/CombatService.cs` + register in `Program.cs`
4. `server/Controllers/CombatController.cs` — `POST /v1/combat/{characterId}/attack`
5. `dotnet ef migrations add AddCombat` from `server/`
6. Unit tests in `server-tests/UnitTests/Services/CombatServiceTests.cs`
7. Update `server/docs/api.md` with the new endpoint
8. Add `client-cli/Services/ApiClient.cs` method `AttackAsync(Guid characterId, ...)`
9. Add option in `GameScreen.cs`, create `CombatScreen.cs` if needed
10. Wire into `Program.cs` navigation loop
11. Add client unit tests in `client-cli-tests/Services/ApiClientTests.cs`
12. Update `client-cli/docs/screens.md`

---

## Common Mistakes to Avoid

- **Scoped service inside a singleton** — `FishingService` was accidentally registered as singleton once, causing it to hold a stale `IInventoryService`. Always register game services as `Scoped`.
- **Trusting userId from the request body** — always read it from the JWT claim.
- **Jumping over variable declarations with goto** — C# forbids this; restructure using a flag or by moving the label above the declaration.
- **Deleting the SQLite DB to fix migration errors** — check if `MigrateAsync` is failing because the schema already exists from an older run; delete `adventure_rpg.db` only in development.
- **Testing with the old coverage results file** — the coverage script picks the latest file with `ls -t | head -1`; stale files from a previous run can skew results.
