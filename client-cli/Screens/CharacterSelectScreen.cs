using AdventureRpgCli.Models;
using AdventureRpgCli.Services;
using Spectre.Console;

namespace AdventureRpgCli.Screens;

public class CharacterSelectScreen(ApiClient api)
{
    private const string CreateNew = "+ Create New Character";
    private const string Logout    = "← Logout";

    /// <summary>Returns the selected character, or null if the user logged out.</summary>
    public async Task<Character?> ShowAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold yellow]Your Characters[/]");
            AnsiConsole.WriteLine();

            List<Character> characters = [];

            try
            {
                await AnsiConsole.Status()
                    .StartAsync("Loading characters...", async _ =>
                        characters = await api.GetCharactersAsync());
            }
            catch (ApiException ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Body.EscapeMarkup()}");
                Pause();
                return null;
            }

            var choices = characters
                .Select(c => $"{c.Name}  [{c.ClassName}]  Lvl {c.Level}")
                .Append(CreateNew)
                .Append(Logout)
                .ToList();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Choose a character or create a new one[/]")
                    .PageSize(12)
                    .AddChoices(choices));

            if (choice == Logout)
            {
                await LogoutAsync();
                return null;
            }

            if (choice == CreateNew)
            {
                var created = await CreateCharacterAsync();
                if (created is not null) return created;
                continue;
            }

            var index = choices.IndexOf(choice);
            return characters[index];
        }
    }

    private async Task<Character?> CreateCharacterAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold yellow]Create Character[/]");
        AnsiConsole.WriteLine();

        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Character name:[/]")
                .Validate(n => n.Length >= 2 && n.Length <= 24
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Name must be 2–24 characters")));

        AnsiConsole.WriteLine();

        var classOptions = new Dictionary<string, int>
        {
            ["Warrior  — High strength, built to endure"]    = 0,
            ["Mage     — High intelligence, mastery of magic"] = 1,
            ["Rogue    — High agility, quick and cunning"]   = 2,
            ["Ranger   — Balanced stats, skilled outdoors"]  = 3,
        };

        var classChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[green]Choose a class:[/]")
                .AddChoices(classOptions.Keys));

        try
        {
            Character? character = null;
            await AnsiConsole.Status()
                .StartAsync("Creating character...", async _ =>
                    character = await api.CreateCharacterAsync(name, classOptions[classChoice]));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold green]{character!.Name} the {character.ClassName} has entered the world.[/]");

            DrawCharacterStats(character);
            await Task.Delay(1500);
            return character;
        }
        catch (ApiException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Body.EscapeMarkup()}");
            Pause();
            return null;
        }
    }

    private async Task LogoutAsync()
    {
        try
        {
            await AnsiConsole.Status()
                .StartAsync("Logging out...", async _ => await api.LogoutAsync());
        }
        catch { /* best effort */ }
    }

    private static void DrawCharacterStats(Character c)
    {
        AnsiConsole.WriteLine();
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[grey]Stat[/]")
            .AddColumn("[grey]Value[/]");

        table.AddRow("Strength",     $"[red]{c.Strength}[/]");
        table.AddRow("Intelligence", $"[blue]{c.Intelligence}[/]");
        table.AddRow("Agility",      $"[green]{c.Agility}[/]");

        AnsiConsole.Write(table);
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);
    }
}
