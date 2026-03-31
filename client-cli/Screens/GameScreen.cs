using AdventureRpgCli.Models;
using AdventureRpgCli.Services;
using Spectre.Console;

namespace AdventureRpgCli.Screens;

public enum GameAction { Continue, BackToCharacters, Logout, DeletedAccount }

public class GameScreen(ApiClient api)
{
    public async Task<GameAction> ShowAsync(Character character)
    {
        while (true)
        {
            AnsiConsole.Clear();
            DrawHeader(character);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]What would you like to do?[/]")
                    .AddChoices(
                        "Go Fishing",
                        "View Inventory",
                        "─────────────────",
                        "Switch Character",
                        "Logout",
                        "Delete Character",
                        "Delete Account"));

            switch (choice)
            {
                case "Go Fishing":
                    await GoFishingAsync(character);
                    break;

                case "View Inventory":
                    await ViewInventoryAsync(character);
                    break;

                case "Switch Character":
                    return GameAction.BackToCharacters;

                case "Logout":
                    await LogoutAsync();
                    return GameAction.Logout;

                case "Delete Character":
                    if (await DeleteCharacterAsync(character))
                        return GameAction.BackToCharacters;
                    break;

                case "Delete Account":
                    if (await DeleteAccountAsync())
                        return GameAction.DeletedAccount;
                    break;
            }
        }
    }

    private async Task GoFishingAsync(Character character)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold yellow]{character.Name}[/] heads to the water's edge...");
        AnsiConsole.WriteLine();

        FishingResult? result = null;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("Casting line...", async _ =>
            {
                await Task.Delay(800);
                result = await api.CastAsync(character.Id);
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]Roll: {result!.Roll} / {result.RequiredRoll} needed[/]");
        AnsiConsole.WriteLine();

        if (result.Success && result.CaughtItem is not null)
        {
            var rarityColor = result.CaughtItem.Rarity switch
            {
                1 => "white",
                2 => "green",
                3 => "blue",
                4 => "gold1",
                _ => "white"
            };

            AnsiConsole.Write(new Panel(
                $"[bold {rarityColor}]{result.CaughtItem.Name}[/]\n" +
                $"[grey]{result.CaughtItem.Description}[/]\n" +
                $"Rarity: [{rarityColor}]{result.CaughtItem.RarityLabel}[/]")
                .Header("[bold green] You caught something! [/]")
                .BorderColor(Color.Green));
        }
        else
        {
            AnsiConsole.Write(new Panel(
                $"[grey]{result.Message}[/]")
                .Header("[yellow] No luck this time [/]")
                .BorderColor(Color.Yellow));
        }

        Pause();
    }

    private async Task ViewInventoryAsync(Character character)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold yellow]{character.Name}'s Inventory[/]");
        AnsiConsole.WriteLine();

        List<Item> items = [];

        await AnsiConsole.Status()
            .StartAsync("Loading inventory...", async _ =>
                items = await api.GetInventoryAsync(character.Id));

        if (items.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Your inventory is empty. Try going fishing![/]");
        }
        else
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("[bold]Item[/]")
                .AddColumn("[bold]Type[/]")
                .AddColumn("[bold]Rarity[/]")
                .AddColumn("[bold]Acquired[/]");

            foreach (var item in items)
            {
                var rarityColor = item.Rarity switch
                {
                    1 => "white",
                    2 => "green",
                    3 => "blue",
                    4 => "gold1",
                    _ => "white"
                };

                table.AddRow(
                    item.Name,
                    $"[grey]{item.Type}[/]",
                    $"[{rarityColor}]{item.RarityLabel}[/]",
                    $"[grey]{item.AcquiredAt:MMM d, yyyy}[/]");
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"\n[grey]{items.Count} item(s) total[/]");
        }

        Pause();
    }

    private async Task<bool> DeleteCharacterAsync(Character character)
    {
        AnsiConsole.WriteLine();
        var confirm = AnsiConsole.Prompt(
            new ConfirmationPrompt(
                $"[red]Delete {character.Name.EscapeMarkup()}? This cannot be undone.[/]")
            { DefaultValue = false });

        if (!confirm) return false;

        try
        {
            await AnsiConsole.Status()
                .StartAsync("Deleting character...", async _ =>
                    await api.DeleteCharacterAsync(character.Id));

            AnsiConsole.MarkupLine($"[grey]{character.Name.EscapeMarkup()} has left the world.[/]");
            await Task.Delay(1000);
            return true;
        }
        catch (ApiException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Body.EscapeMarkup()}");
            Pause();
            return false;
        }
    }

    private async Task LogoutAsync()
    {
        await AnsiConsole.Status()
            .StartAsync("Logging out...", async _ => await api.LogoutAsync());

        AnsiConsole.MarkupLine("[grey]Farewell, adventurer.[/]");
        await Task.Delay(800);
    }

    private async Task<bool> DeleteAccountAsync()
    {
        AnsiConsole.WriteLine();
        var confirm = AnsiConsole.Prompt(
            new ConfirmationPrompt(
                "[red]Delete your account? All characters and data will be permanently lost.[/]")
            { DefaultValue = false });

        if (!confirm) return false;

        try
        {
            await AnsiConsole.Status()
                .StartAsync("Deleting account...", async _ =>
                    await api.DeleteAccountAsync());

            AnsiConsole.MarkupLine("[grey]Your account has been deleted. Farewell.[/]");
            await Task.Delay(1200);
            return true;
        }
        catch (ApiException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Body.EscapeMarkup()}");
            Pause();
            return false;
        }
    }

    private static void DrawHeader(Character character)
    {
        var panel = new Panel(
            $"[bold]{character.Name.EscapeMarkup()}[/]  [grey]|[/]  {character.ClassName}  [grey]|[/]  Level {character.Level}\n" +
            $"[red]STR {character.Strength}[/]   [blue]INT {character.Intelligence}[/]   [green]AGI {character.Agility}[/]")
            .BorderColor(Color.Gold1)
            .Padding(1, 0);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);
    }
}
