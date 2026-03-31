# AdventureRpg

An old-school menu-based RPG. Create a character, go fishing, build your inventory.

## Project Structure

```
server/         .NET 10 REST API
server-tests/   Unit tests + coverage tooling
docs/           Project-level documentation
```

## Getting Started

### Run the server

```bash
cd server
dotnet run
```

API available at `http://localhost:5000`
Scalar API docs at `http://localhost:5000/scalar`

### Run tests

```bash
cd server-tests/UnitTests
dotnet test
```

### Check branch coverage

```bash
./server-tests/coverage-branch.sh
```

Outputs a percentage. Target: **≥ 80%**

## Docs

- [Server overview](server/docs/overview.md)
- [API reference](server/docs/api.md)
- [Characters](server/docs/characters.md)
- [Fishing](server/docs/fishing.md)
- [Auth](server/docs/auth.md)
