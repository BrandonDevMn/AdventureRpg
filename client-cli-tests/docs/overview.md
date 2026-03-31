# client-cli-tests

Unit tests for the `client-cli` project. Tests run without a live server — all HTTP calls are intercepted by a `MockHttpHandler` that returns pre-configured responses.

## Tech Stack

- **Framework:** xUnit
- **HTTP mocking:** `MockHttpHandler` (custom `HttpMessageHandler` subclass)
- **Coverage:** coverlet — target ≥ 80% branch coverage on testable code

## What Is Tested

| Area | Coverage |
|---|---|
| `Models` — `Character.ClassName`, `Item.RarityLabel` | All branches |
| `ApiClient` — auth, characters, fishing, inventory | All branches |
| `ApiClient` — auto-refresh on 401 | All scenarios |
| Screens | Excluded — depend on Spectre.Console static API |

## Project Structure

```
client-cli-tests/
  Helpers/
    MockHttpHandler.cs     — queues predetermined HTTP responses
    ApiClientFactory.cs    — creates ApiClient wired to MockHttpHandler
  Models/
    ModelTests.cs          — ClassName and RarityLabel computed properties
  Services/
    ApiClientTests.cs      — all ApiClient methods and token refresh logic
  coverage.runsettings     — excludes Screens and Program from coverage
  docs/                    — this documentation
```

## Running Tests

```bash
dotnet test
```

## Checking Branch Coverage

```bash
dotnet test --settings coverage.runsettings --collect:"XPlat Code Coverage"
```
