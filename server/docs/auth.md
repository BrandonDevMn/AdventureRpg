# Auth

Authentication uses ASP.NET Core Identity with JWT Bearer tokens. All game endpoints require a valid token.

## Endpoints

### Register
`POST /v1/auth/register`

Creates a new account and returns a JWT.

**Request body:**
```json
{
  "email": "hero@adventure.com",
  "password": "mysecret"
}
```

**Password rules:** minimum 6 characters. No uppercase or special character requirement.

**Response `200 OK`:**
```json
{
  "token": "eyJhbGci...",
  "userId": "a1b2c3...",
  "expiresAt": "2026-03-31T02:00:00Z"
}
```

**Response `400 Bad Request`:** validation errors (e.g. email already in use)

---

### Login
`POST /v1/auth/login`

**Request body:**
```json
{
  "email": "hero@adventure.com",
  "password": "mysecret"
}
```

**Response `200 OK`:** same shape as register

**Response `401 Unauthorized`:** invalid email or password

---

## Using the Token

Pass the token in the `Authorization` header on all game requests:

```
Authorization: Bearer eyJhbGci...
```

## Token Expiry

Tokens expire after 60 minutes by default (configured in `appsettings.json`). After expiry, log in again to get a new token.

## Configuration

JWT settings live in `appsettings.json`:

```json
"Jwt": {
  "Key": "...",
  "Issuer": "AdventureRpg",
  "Audience": "AdventureRpg",
  "ExpiryMinutes": "60"
}
```

**The `Key` must be replaced with a strong secret before deploying.** Use environment variables or .NET user secrets to override it outside of development.
