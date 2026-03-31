# ApiClient

`Services/ApiClient.cs` handles all communication with the AdventureRpg server. It wraps every endpoint in a typed method, manages JWT tokens in memory, and automatically refreshes the access token when it expires.

## Token Management

After a successful register or login the client stores two tokens:

| Token | Lifetime | Purpose |
|---|---|---|
| Access token | 60 minutes | Sent on every request as `Authorization: Bearer <token>` |
| Refresh token | Until logout | Used to obtain a new access token when the current one expires |

Tokens are held in private fields. The refresh token is also persisted to disk by `SessionStore` so the session survives process restarts.

## Auto-Refresh

Every protected request goes through `SendAuthorized`. If the server returns `401 Unauthorized`:

1. `SendAuthorized` calls `POST /v1/auth/refresh` with the stored refresh token
2. On success the new tokens are stored and the original request is retried once
3. On failure an `ApiException` is thrown

## Session Persistence

`ApiClient` accepts an optional `SessionStore` that it uses to persist the refresh token between runs.

| Event | Action |
|---|---|
| Login / Register / Refresh | Saves refresh token to `~/.adventurerpg/session` |
| Logout / Delete account | Deletes `~/.adventurerpg/session` |

On startup, `Program.cs` calls `api.RestoreSessionAsync(savedToken)`. This calls `POST /v1/auth/refresh` and, on success, populates both tokens so the welcome screen can be skipped.

## Methods

| Method | Endpoint |
|---|---|
| `RegisterAsync` | `POST /v1/auth/register` |
| `LoginAsync` | `POST /v1/auth/login` |
| `LogoutAsync` | `POST /v1/auth/logout` |
| `RestoreSessionAsync` | `POST /v1/auth/refresh` (session restore on startup) |
| `DeleteAccountAsync` | `DELETE /v1/auth/account` |
| `GetCharactersAsync` | `GET /v1/character` |
| `CreateCharacterAsync` | `POST /v1/character` |
| `DeleteCharacterAsync` | `DELETE /v1/character/{id}` |
| `CastAsync` | `POST /v1/fishing/{id}/cast` |
| `GetInventoryAsync` | `GET /v1/inventory/{id}` |

## Error Handling

All non-success responses throw `ApiException` which carries the HTTP status code and raw response body. Screens catch this and display a friendly error message.

## Configuration

The server base URL is read from `appsettings.json`:

```json
{ "ServerUrl": "http://localhost:5000" }
```
