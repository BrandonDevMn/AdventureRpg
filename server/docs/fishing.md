# Fishing

Fishing is an activity where a character casts their line and rolls the dice to see if they catch something — and if so, what.

## How It Works

1. A d100 is rolled (1–100)
2. A required roll is calculated: `50 - max(0, (Agility - 5) * 2)`
3. If the roll is **at or below** the required roll, a fish is caught
4. A second weighted roll selects which fish from the loot table

Higher Agility lowers the required roll, increasing catch chance by 2% per point above 5.

**Example:** A Ranger (Agility 9) has a required roll of `50 - (4 * 2) = 42`, meaning a 42% catch chance.

## Fish Loot Table

| Fish              | Rarity    | Notes                                  |
|-------------------|-----------|----------------------------------------|
| Minnow            | Common    | Most common catch                      |
| Perch             | Common    |                                        |
| Old Boot          | Common    | Junk item                              |
| Bass              | Uncommon  |                                        |
| Catfish           | Uncommon  |                                        |
| Golden Trout      | Rare      |                                        |
| Shadow Eel        | Rare      |                                        |
| Leviathan Fry     | Legendary | Extremely rare                         |

## Inventory

When a fish is caught it is automatically added to the character's inventory. Failed casts add nothing.
