using AdventureRpg.Models;
using AdventureRpg.Services;
using Moq;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

// Fish loot table cumulative weight boundaries (total = 103):
//   Old Boot   rarity=1 Junk  : 1-15
//   Minnow     rarity=1 Fish  : 16-45
//   Perch      rarity=1 Fish  : 46-70
//   Bass       rarity=2 Fish  : 71-85
//   Catfish    rarity=2 Fish  : 86-95
//   Golden Trout rarity=3 Fish: 96-99
//   Shadow Eel  rarity=3 Fish : 100-102
//   Leviathan Fry rarity=4   : 103

public class FishingServiceTests
{
    private readonly Mock<IInventoryService> _inventoryMock = new();

    private FishingService CreateService(params int[] rolls) =>
        new(_inventoryMock.Object, new ControlledRandom(rolls));

    private static Character CharacterWithAgility(int agility) => new()
    {
        Name = "Tester",
        Class = CharacterClass.Ranger,
        Agility = agility
    };

    // ── Miss / catch ──────────────────────────────────────────────────────────

    [Fact]
    public void Cast_RollExceedsRequired_ReturnsMiss()
    {
        // agility=5 → requiredRoll=50; roll=51 → miss
        var service = CreateService(51);
        var result = service.Cast(CharacterWithAgility(5));

        Assert.False(result.Success);
        Assert.Null(result.CaughtItem);
        Assert.Equal(51, result.Roll);
        Assert.Equal(50, result.RequiredRoll);
    }

    [Fact]
    public void Cast_RollWithinRequired_ReturnsCatch()
    {
        // agility=5 → requiredRoll=50; roll=1 → catch; fish roll=20 → Minnow
        var service = CreateService(1, 20);
        var result = service.Cast(CharacterWithAgility(5));

        Assert.True(result.Success);
        Assert.NotNull(result.CaughtItem);
    }

    // ── Item type (Junk vs Fish) ──────────────────────────────────────────────

    [Fact]
    public void Cast_CatchOldBoot_TypeIsJunk()
    {
        // fish roll=1 → Old Boot (rarity 1, name "Old Boot" → Junk)
        var service = CreateService(1, 1);
        var result = service.Cast(CharacterWithAgility(5));

        Assert.True(result.Success);
        Assert.Equal("Old Boot", result.CaughtItem!.Name);
    }

    [Fact]
    public void Cast_CatchRegularFish_TypeIsFish()
    {
        // fish roll=20 → Minnow (rarity 1, not "Old Boot" → Fish)
        var service = CreateService(1, 20);
        var result = service.Cast(CharacterWithAgility(5));

        Assert.True(result.Success);
        Assert.Equal("Minnow", result.CaughtItem!.Name);
    }

    // ── Rarity labels ────────────────────────────────────────────────────────

    [Fact]
    public void Cast_CommonFish_RarityLabelIsCommon()
    {
        var service = CreateService(1, 20); // Minnow rarity=1
        var result = service.Cast(CharacterWithAgility(5));
        Assert.Equal("Common", result.CaughtItem!.RarityLabel);
    }

    [Fact]
    public void Cast_UncommonFish_RarityLabelIsUncommon()
    {
        var service = CreateService(1, 75); // Bass rarity=2
        var result = service.Cast(CharacterWithAgility(5));
        Assert.Equal("Uncommon", result.CaughtItem!.RarityLabel);
    }

    [Fact]
    public void Cast_RareFish_RarityLabelIsRare()
    {
        var service = CreateService(1, 97); // Golden Trout rarity=3
        var result = service.Cast(CharacterWithAgility(5));
        Assert.Equal("Rare", result.CaughtItem!.RarityLabel);
    }

    [Fact]
    public void Cast_LegendaryFish_RarityLabelIsLegendary()
    {
        var service = CreateService(1, 103); // Leviathan Fry rarity=4
        var result = service.Cast(CharacterWithAgility(5));
        Assert.Equal("Legendary", result.CaughtItem!.RarityLabel);
    }

    // ── Agility bonus ─────────────────────────────────────────────────────────

    [Fact]
    public void Cast_AgilityAtOrBelowFive_NoBonus_RequiredRollIsFifty()
    {
        var service = CreateService(51); // just above 50 → miss confirms requiredRoll=50
        var result = service.Cast(CharacterWithAgility(5));
        Assert.Equal(50, result.RequiredRoll);
    }

    [Fact]
    public void Cast_AgilityAboveFive_ReducesRequiredRoll()
    {
        // agility=10 → bonus=(10-5)*2=10 → requiredRoll=40
        var service = CreateService(99); // guaranteed miss regardless
        var result = service.Cast(CharacterWithAgility(10));
        Assert.Equal(40, result.RequiredRoll);
    }

    [Fact]
    public void Cast_ExtremeAgility_RequiredRollFloorIsOne()
    {
        // agility=30 → bonus=50 → 50-50=0 → clamped to 1
        var service = CreateService(99);
        var result = service.Cast(CharacterWithAgility(30));
        Assert.Equal(1, result.RequiredRoll);
    }

    // ── Inventory integration ─────────────────────────────────────────────────

    [Fact]
    public void Cast_OnCatch_AddsItemToInventory()
    {
        var service = CreateService(1, 20); // catch Minnow
        var character = CharacterWithAgility(5);

        service.Cast(character);

        _inventoryMock.Verify(i => i.AddItem(character.Id, It.IsAny<Item>()), Times.Once);
    }

    [Fact]
    public void Cast_OnMiss_DoesNotAddItemToInventory()
    {
        var service = CreateService(51); // miss
        var character = CharacterWithAgility(5);

        service.Cast(character);

        _inventoryMock.Verify(i => i.AddItem(It.IsAny<Guid>(), It.IsAny<Item>()), Times.Never);
    }
}
