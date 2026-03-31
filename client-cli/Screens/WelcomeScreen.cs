using AdventureRpgCli.Services;
using Spectre.Console;

namespace AdventureRpgCli.Screens;

public class WelcomeScreen(ApiClient api)
{
    public async Task<bool> ShowAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            DrawTitle();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Use arrow keys to navigate, Enter to select[/]")
                    .AddChoices("Create Account", "Login", "Quit"));

            switch (choice)
            {
                case "Create Account":
                    if (await RegisterAsync()) return true;
                    break;
                case "Login":
                    if (await LoginAsync()) return true;
                    break;
                case "Quit":
                    return false;
            }
        }
    }

    private async Task<bool> RegisterAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold yellow]Create Account[/]");
        AnsiConsole.WriteLine();

        var email = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Email:[/]")
                .Validate(e => e.Contains('@') ? ValidationResult.Success() : ValidationResult.Error("Enter a valid email")));

        var password = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Password:[/]")
                .Secret()
                .Validate(p => p.Length >= 6 ? ValidationResult.Success() : ValidationResult.Error("Minimum 6 characters")));

        try
        {
            await AnsiConsole.Status()
                .StartAsync("Creating account...", async _ => await api.RegisterAsync(email, password));

            AnsiConsole.MarkupLine("[bold green]Account created! Welcome, adventurer.[/]");
            await Task.Delay(1000);
            return true;
        }
        catch (ApiException ex)
        {
            AnsiConsole.MarkupLine($"[red]Registration failed:[/] {ex.Body.EscapeMarkup()}");
            Pause();
            return false;
        }
    }

    private async Task<bool> LoginAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold yellow]Login[/]");
        AnsiConsole.WriteLine();

        var email = AnsiConsole.Prompt(new TextPrompt<string>("[green]Email:[/]"));
        var password = AnsiConsole.Prompt(new TextPrompt<string>("[green]Password:[/]").Secret());

        try
        {
            await AnsiConsole.Status()
                .StartAsync("Logging in...", async _ => await api.LoginAsync(email, password));

            AnsiConsole.MarkupLine("[bold green]Welcome back, adventurer.[/]");
            await Task.Delay(1000);
            return true;
        }
        catch (ApiException)
        {
            AnsiConsole.MarkupLine("[red]Invalid email or password.[/]");
            Pause();
            return false;
        }
    }

    private static void DrawTitle()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new FigletText("Adventure RPG").Color(Color.Gold1));
        AnsiConsole.Write(new Rule("[grey]An old-school text adventure[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);
    }
}
