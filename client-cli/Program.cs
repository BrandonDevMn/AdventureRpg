using AdventureRpgCli.Screens;
using AdventureRpgCli.Services;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var sessionStore = new SessionStore();
var api = new ApiClient(config["ServerUrl"]!, sessionStore);
var welcome = new WelcomeScreen(api);
var characterSelect = new CharacterSelectScreen(api);
var game = new GameScreen(api);

// Try to restore a previous session from disk
var savedToken = sessionStore.Load();
if (savedToken is not null)
{
    await AnsiConsole.Status()
        .StartAsync("Restoring session...", _ => api.RestoreSessionAsync(savedToken));
}

while (true)
{
    if (!api.IsLoggedIn)
    {
        var loggedIn = await welcome.ShowAsync();
        if (!loggedIn) break;
    }

    // Character select loop
    while (api.IsLoggedIn)
    {
        var character = await characterSelect.ShowAsync();
        if (character is null) break; // logged out from character screen

        // Game loop for this character
        while (true)
        {
            var action = await game.ShowAsync(character);

            if (action == GameAction.BackToCharacters) break;
            if (action == GameAction.Logout || action == GameAction.DeletedAccount) goto NextLogin;
        }
    }

    NextLogin:;
}

AnsiConsole.Clear();
AnsiConsole.MarkupLine("[grey]Thanks for playing. Goodbye![/]");
AnsiConsole.WriteLine();
