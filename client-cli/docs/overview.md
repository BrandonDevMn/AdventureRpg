# client-cli

A .NET 10 terminal client for AdventureRpg. Lets a player register, log in, create characters, go fishing, manage their inventory, and delete their account — all from an interactive command-line interface.

## Tech Stack

- **Runtime:** .NET 10 console application
- **UI:** Spectre.Console — menus, tables, panels, spinners, color
- **Config:** `appsettings.json` — server URL

## Project Structure

```
client-cli/
  Program.cs              — entry point and main navigation loop
  appsettings.json        — server URL configuration
  Models/
    Models.cs             — response records (Character, Item, FishingResult, etc.)
  Services/
    ApiClient.cs          — HTTP client wrapping all server API calls
    SessionStore.cs       — persists refresh token to ~/.adventurerpg/session
  Screens/
    WelcomeScreen.cs      — title screen, register and login
    CharacterSelectScreen.cs — list, create and select characters
    GameScreen.cs         — main game menu (fishing, inventory, delete, logout)
  docs/                   — this documentation
```

## Documentation

- [ApiClient](api-client.md) — how HTTP calls and token refresh work
- [Screens](screens.md) — the screen flow and navigation
