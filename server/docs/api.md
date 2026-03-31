# API Reference

Base URL: `http://localhost:5000`

---

## Characters

### Create Character
`POST /api/character`

Creates a new character. Stats are assigned automatically based on class.

**Request body:**
```json
{
  "name": "Aldric",
  "class": 0
}
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
  "createdAt": "2026-03-30T00:00:00Z"
}
```

---

### Get Character
`GET /api/character/{id}`

Returns a single character by ID.

**Response `200 OK`:** Character object (see above)
**Response `404 Not Found`:** Character does not exist

---

### List All Characters
`GET /api/character`

Returns all characters.

**Response `200 OK`:** Array of character objects

---

## Fishing

### Cast Line
`POST /api/fishing/{characterId}/cast`

Rolls the dice and attempts to catch a fish. On success, the caught item is added to the character's inventory automatically.

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

**Response `404 Not Found`:** Character does not exist

---

## Inventory

### Get Inventory
`GET /api/inventory/{characterId}`

Returns all items in a character's inventory.

**Response `200 OK`:**
```json
{
  "characterId": "a1b2c3...",
  "items": [
    {
      "id": "d4e5f6...",
      "name": "Perch",
      "type": 0,
      "description": "A common river fish.",
      "rarity": 1,
      "acquiredAt": "2026-03-30T00:00:00Z"
    }
  ]
}
```

**Response `404 Not Found`:** Character does not exist
