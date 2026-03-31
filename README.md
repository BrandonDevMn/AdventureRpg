# AdventureRpg

An old-school menu-based RPG. Create an account, build a character, go fishing, and manage your inventory — all from a terminal.

## Projects

| Folder | Type | Description |
|---|---|---|
| `server/` | .NET 10 REST API | Game backend — auth, characters, fishing, inventory |
| `server-tests/UnitTests/` | xUnit | Unit tests for the server (no live server needed) |
| `server-tests/ApiTests/` | xUnit | End-to-end flow tests (requires live server) |
| `client-cli/` | .NET 10 console app | Interactive terminal game client |
| `client-cli-tests/` | xUnit | Unit tests for the client (no live server needed) |

## Quick Start

### 1. Start the server

```bash
cd server
dotnet run
```

Server runs at `http://localhost:5000`
API explorer (Scalar) at `http://localhost:5000/scalar`

### 2. Play the game

```bash
cd client-cli
dotnet run
```

### 3. Run all unit tests

```bash
# Server unit tests
cd server-tests/UnitTests && dotnet test

# Client unit tests
cd client-cli-tests && dotnet test
```

### 4. Run end-to-end tests (server must be running)

```bash
cd server-tests/ApiTests && dotnet test
```

### 5. Check branch coverage

```bash
# Server — prints a percentage, target ≥ 80%
./server-tests/coverage-branch.sh
```

## Project Docs

### Project-wide
- [Maintainer Guide](docs/maintainer.md)

### Server
- [Overview](server/docs/overview.md)
- [API Reference](server/docs/api.md)
- [Auth](server/docs/auth.md)
- [Characters](server/docs/characters.md)
- [Fishing](server/docs/fishing.md)

### Client CLI
- [Overview](client-cli/docs/overview.md)
- [ApiClient](client-cli/docs/api-client.md)
- [Screens](client-cli/docs/screens.md)

### Tests
- [Server Unit Tests](server-tests/UnitTests/docs/overview.md)
- [Server API Tests](server-tests/ApiTests/docs/overview.md)
- [Client Unit Tests](client-cli-tests/docs/overview.md)
