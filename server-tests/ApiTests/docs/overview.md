# ApiTests

End-to-end flow tests for the AdventureRpg server. These tests run against a live server and exercise the full game flow over real HTTP.

## Requirements

The server must be running before these tests are executed:

```bash
cd server && dotnet run
```

The target URL is configured in `appsettings.json`:

```json
{ "ServerUrl": "http://localhost:5000" }
```

## Tech Stack

- **Framework:** xUnit
- **HTTP:** `System.Net.Http` / `HttpClient`
- **Config:** `Microsoft.Extensions.Configuration.Json`

## What Is Tested

A single `[Fact]` named `FullGameFlow` runs the complete player journey in order:

| Step | Assertion |
|---|---|
| Register | `200 OK`, access token and refresh token returned |
| Login | `200 OK`, fresh tokens returned |
| Create character | `201 Created`, name and class correct |
| List characters | `200 OK`, exactly one character present |
| Go fishing | `200 OK`, roll values in valid range, item fields populated on a catch |
| Delete character | `204 No Content`, `404` confirmed afterwards |
| Delete account | `204 No Content`, `401` confirmed when no token is sent |

Each run uses a unique `test-{guid}@adventure.com` email so the test is safe to run repeatedly against the same server.

## Running Tests

```bash
dotnet test
```
