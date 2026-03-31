using AdventureRpg.Models;
using AdventureRpg.DTOs;

namespace AdventureRpg.Services;

public interface IFishingService
{
    FishingResultResponse Cast(Character character);
}

public class FishingService : IFishingService
{
    private readonly IInventoryService _inventoryService;
    private readonly Random _random;

    // (name, description, rarity, weight) — higher weight = more likely when a fish IS caught
    private static readonly (string name, string description, int rarity, int weight)[] FishTable =
    [
        ("Old Boot",          "Someone's soggy boot. Still has a foot-smell.",   1, 15),
        ("Minnow",            "A tiny, unremarkable fish.",                       1, 30),
        ("Perch",             "A common river fish.",                             1, 25),
        ("Bass",              "A decent catch for any adventurer.",               2, 15),
        ("Catfish",           "A whiskered bottom-dweller.",                      2, 10),
        ("Golden Trout",      "Shimmers like a coin in the water.",               3,  4),
        ("Shadow Eel",        "Slippery and unnervingly cold to the touch.",      3,  3),
        ("Leviathan Fry",     "A juvenile deep-sea horror. Still terrifying.",    4,  1),
    ];

    // Base catch chance is 50%; Agility boosts it by 2% per point above 5
    private const int BaseCatchChance = 50;
    private const int DiceSides = 100;

    public FishingService(IInventoryService inventoryService, Random? random = null)
    {
        _inventoryService = inventoryService;
        _random = random ?? new Random();
    }

    public FishingResultResponse Cast(Character character)
    {
        int bonus = Math.Max(0, character.Agility - 5) * 2;
        int requiredRoll = Math.Max(1, BaseCatchChance - bonus);
        int roll = _random.Next(1, DiceSides + 1);

        if (roll > requiredRoll)
        {
            return new FishingResultResponse
            {
                Success = false,
                Message = "The line goes taut... then nothing. The fish got away.",
                Roll = roll,
                RequiredRoll = requiredRoll
            };
        }

        var fish = RollFishTable();
        var item = new Item
        {
            Name = fish.name,
            Description = fish.description,
            Type = fish.rarity == 1 && fish.name == "Old Boot" ? ItemType.Junk : ItemType.Fish,
            Rarity = fish.rarity
        };

        _inventoryService.AddItem(character.Id, item);

        return new FishingResultResponse
        {
            Success = true,
            Message = $"You reeled in a {item.Name}!",
            Roll = roll,
            RequiredRoll = requiredRoll,
            CaughtItem = new FishCaughtDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Rarity = item.Rarity,
                RarityLabel = item.Rarity switch
                {
                    1 => "Common",
                    2 => "Uncommon",
                    3 => "Rare",
                    4 => "Legendary",
                    _ => "Unknown"
                }
            }
        };
    }

    private (string name, string description, int rarity, int weight) RollFishTable()
    {
        int totalWeight = FishTable.Sum(f => f.weight);
        int roll = _random.Next(1, totalWeight + 1);
        int cumulative = 0;
        foreach (var fish in FishTable)
        {
            cumulative += fish.weight;
            if (roll <= cumulative)
                return fish;
        }
        return FishTable[0];
    }
}
