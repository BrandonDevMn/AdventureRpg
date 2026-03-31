namespace AdventureRpgCli.Models;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string UserId,
    DateTime AccessTokenExpiresAt);

public record Character(
    Guid Id,
    string Name,
    int Class,
    int Level,
    int Strength,
    int Intelligence,
    int Agility)
{
    public string ClassName => Class switch
    {
        0 => "Warrior",
        1 => "Mage",
        2 => "Rogue",
        3 => "Ranger",
        _ => "Unknown"
    };
}

public record FishingResult(
    bool Success,
    string Message,
    int Roll,
    int RequiredRoll,
    CaughtItem? CaughtItem);

public record CaughtItem(
    Guid Id,
    string Name,
    string Description,
    int Rarity,
    string RarityLabel);

public record Item(
    Guid Id,
    string Name,
    string Type,
    string Description,
    int Rarity,
    DateTime AcquiredAt)
{
    public string RarityLabel => Rarity switch
    {
        1 => "Common",
        2 => "Uncommon",
        3 => "Rare",
        4 => "Legendary",
        _ => "Unknown"
    };
}

public record InventoryResponse(Guid CharacterId, List<Item> Items);
