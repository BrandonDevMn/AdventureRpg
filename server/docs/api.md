# API Reference

Base URL: `http://localhost:5000`

All endpoints except auth require an `Authorization: Bearer <token>` header. See [Auth](auth.md) for how to obtain a token.

---

## Auth

### Register
`POST /v1/auth/register`

**Request body:**
```json
{ "email": "hero@adventure.com", "password": "mysecret" }
```

**Response `200 OK`:**
```json
{ "token": "eyJhbGci...", "userId": "a1b2c3...", "expiresAt": "2026-03-31T02:00:00Z" }
```

**Response `400 Bad Request`:** identity validation errors

---

### Login
`POST /v1/auth/login`

**Request body:**
```json
{ "email": "hero@adventure.com", "password": "mysecret" }
```

**Response `200 OK`:** same as register

**Response `401 Unauthorized`:** invalid credentials

---

## Characters

### Create Character
`POST /v1/character`

Creates a character owned by the authenticated user.

**Request body:**
```json
{ "name": "Aldric", "class": 0 }
```

`class` values: `0` = Warrior, `1` = Mage, `2` = Rogue, `3` = Ranger

**Response `201 Created`:**
```json
{
  "id": "a1b2c3...",
  "name": "Aldric",
  "class": 0,
  "level": 1,
  "strength": 10,
  "intelligence": 4,
  "agility": 6,
  "userId": "u1...",
  "createdAt": "2026-03-31T00:00:00Z"
}
```

---

### Get Character
`GET /v1/character/{id}`

Returns the character only if it belongs to the authenticated user.

**Response `200 OK`:** character object
**Response `404 Not Found`:** not found or belongs to another user

---

### List Characters
`GET /v1/character`

Returns all characters belonging to the authenticated user.

**Response `200 OK`:** array of character objects

---

## Fishing

### Cast Line
`POST /v1/fishing/{characterId}/cast`

Rolls the dice and attempts to catch a fish. On success, the item is added to the character's inventory automatically.

**Response `200 OK` (miss):**
```json
{
  "success": false,
  "message": "The line goes taut... then nothing. The fish got away.",
  "roll": 73,
  "requiredRoll": 48,
  "caughtItem": null
}
```

**Response `200 OK` (catch):**
```json
{
  "success": true,
  "message": "You reeled in a Golden Trout!",
  "roll": 12,
  "requiredRoll": 48,
  "caughtItem": {
    "id": "d4e5f6...",
    "name": "Golden Trout",
    "description": "Shimmers like a coin in the water.",
    "rarity": 3,
    "rarityLabel": "Rare"
  }
}
```

**Response `404 Not Found`:** character not found or belongs to another user

---

## Inventory

### Get Inventory
`GET /v1/inventory/{characterId}`

Returns all items in a character's inventory.

**Response `200 OK`:**
```json
{
  "characterId": "a1b2c3...",
  "items": [
    {
      "id": "d4e5f6...",
      "name": "Perch",
      "type": "Fish",
      "description": "A common river fish.",
      "rarity": 1,
      "acquiredAt": "2026-03-31T00:00:00Z"
    }
  ]
}
```

**Response `404 Not Found`:** character not found or belongs to another user
