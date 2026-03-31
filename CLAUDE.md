# Claude Code Instructions

## Documentation

Every subfolder with a `.csproj` has a `docs/` folder with `.md` files describing the project.
The root `docs/` folder contains project-wide guidance.

**Always read and follow these docs before making changes:**

| Document | What it covers |
|---|---|
| [`docs/maintainer.md`](docs/maintainer.md) | Patterns for every layer — server, client, tests. Read this before adding any feature. |
| [`server/docs/overview.md`](server/docs/overview.md) | Server tech stack and structure |
| [`server/docs/api.md`](server/docs/api.md) | All API endpoints — keep this up to date when endpoints change |
| [`server/docs/auth.md`](server/docs/auth.md) | Auth flow and token rules |
| [`server/docs/characters.md`](server/docs/characters.md) | Character classes and stats |
| [`server/docs/fishing.md`](server/docs/fishing.md) | Fishing mechanic and loot table |
| [`client-cli/docs/overview.md`](client-cli/docs/overview.md) | Client app structure |
| [`client-cli/docs/api-client.md`](client-cli/docs/api-client.md) | ApiClient usage, token management, session persistence |
| [`client-cli/docs/screens.md`](client-cli/docs/screens.md) | Screen navigation flow |
| [`server-tests/UnitTests/docs/overview.md`](server-tests/UnitTests/docs/overview.md) | Server unit test structure and coverage target |
| [`server-tests/ApiTests/docs/overview.md`](server-tests/ApiTests/docs/overview.md) | End-to-end API test requirements |
| [`client-cli-tests/docs/overview.md`](client-cli-tests/docs/overview.md) | Client unit test structure and coverage target |

## Key Rules

- **Coverage target: ≥ 80% branch coverage** on both server and client. Run `server-tests/coverage-branch.sh` to check the server. Check client with `dotnet build && dotnet test --no-build --settings coverage.runsettings --collect:"XPlat Code Coverage"` from `client-cli-tests/`.
- **Update docs** whenever you change behaviour — endpoint added, screen changed, new service, etc.
- **Services are Scoped** — never register a service that depends on a scoped service as Singleton.
- **Auth scoping** — every endpoint that touches user data reads `userId` from the JWT claim, never from the request body.
- **No logging framework** — neither the server nor the client currently use a logging framework. Do not add one unless asked.
