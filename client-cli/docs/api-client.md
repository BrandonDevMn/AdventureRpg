# ApiClient

`Services/ApiClient.cs` handles all communication with the AdventureRpg server. It wraps every endpoint in a typed method, manages JWT tokens in memory, and automatically refreshes the access token when it expires.

## Token Management

After a successful register or login the client stores two tokens:

| Token | Lifetime | Purpose |
|---|---|---|
| Access token | 60 minutes | Sent on every request as `Authorization: Bearer <token>` |
| Refresh token | Until logout | Used to obtain a new access token when the current one expires |

Tokens are held in private fields and are never written to disk.

## Auto-Refresh

Every protected request goes through `SendAuthorized`. If the server returns `401 Unauthorized`:

1. `SendAuthorized` calls `POST /v1/auth/refresh` with the stored refresh token
2. On success the new tokens are stored and the original request is retried once
3. On failure an `ApiException` is thrown

## Methods

| Method | Endpoint |
|---|---|
| `RegisterAsync` | `POST /v1/auth/register` |
| `LoginAsync` | `POST /v1/auth/login` |
| `LogoutAsync` | `POST /v1/auth/logout` |
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
