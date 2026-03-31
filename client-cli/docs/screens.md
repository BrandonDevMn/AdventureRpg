# Screens

The UI is split into three screen classes. `Program.cs` orchestrates navigation between them.

## Navigation Flow

```
WelcomeScreen
  ├── Register → CharacterSelectScreen
  ├── Login    → CharacterSelectScreen
  └── Quit     → exit

CharacterSelectScreen
  ├── Select character → GameScreen
  ├── Create character → GameScreen
  └── Logout          → WelcomeScreen

GameScreen
  ├── Go Fishing       → (stays in GameScreen)
  ├── View Inventory   → (stays in GameScreen)
  ├── Switch Character → CharacterSelectScreen
  ├── Logout           → WelcomeScreen
  ├── Delete Character → CharacterSelectScreen
  └── Delete Account   → WelcomeScreen
```

## WelcomeScreen

Displays the "Adventure RPG" ASCII title and a main menu. Handles the register and login forms with inline validation and a spinner during the API call.

## CharacterSelectScreen

Lists all characters belonging to the logged-in user. Each entry shows the character name, class and level. Offers options to create a new character or log out.

**Character creation** prompts for a name (2–24 chars) and a class with plain-English descriptions of each. On success shows a stat table before entering the game.

## GameScreen

The main play loop for a selected character. Shows a character panel header with name, class, level and stats on every menu.

| Option | Description |
|---|---|
| Go Fishing | Casts the line with a spinner, shows the dice roll and result in a colored panel |
| View Inventory | Renders all items in a table with rarity-colored labels |
| Switch Character | Returns to character select |
| Logout | Revokes the refresh token and returns to welcome |
| Delete Character | Confirms then deletes, returns to character select |
| Delete Account | Confirms then deletes all data and returns to welcome |

### Rarity Colors

| Rarity | Color |
|---|---|
| Common | White |
| Uncommon | Green |
| Rare | Blue |
| Legendary | Gold |
