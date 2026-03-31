using AdventureRpg.Models;
using AdventureRpg.Services;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class InventoryServiceTests
{
    private InventoryService CreateService(string dbName) =>
        new(DbContextFactory.Create(dbName));

    [Fact]
    public void GetInventory_NoItems_ReturnsEmpty()
    {
        var service = CreateService("inv_empty");

        var items = service.GetInventory(Guid.NewGuid());

        Assert.Empty(items);
    }

    [Fact]
    public void AddItem_SetsCharacterIdAndPersists()
    {
        var service = CreateService("inv_add");
        var characterId = Guid.NewGuid();
        var item = new Item { Name = "Perch", Type = ItemType.Fish, Rarity = 1 };

        service.AddItem(characterId, item);
        var inventory = service.GetInventory(characterId).ToList();

        Assert.Single(inventory);
        Assert.Equal(characterId, inventory[0].CharacterId);
        Assert.Equal("Perch", inventory[0].Name);
    }

    [Fact]
    public void GetInventory_ReturnsOnlyItemsForSpecifiedCharacter()
    {
        var service = CreateService("inv_isolation");
        var characterA = Guid.NewGuid();
        var characterB = Guid.NewGuid();

        service.AddItem(characterA, new Item { Name = "Minnow", Type = ItemType.Fish, Rarity = 1 });
        service.AddItem(characterB, new Item { Name = "Bass", Type = ItemType.Fish, Rarity = 2 });

        var inventoryA = service.GetInventory(characterA).ToList();

        Assert.Single(inventoryA);
        Assert.Equal("Minnow", inventoryA[0].Name);
    }

    [Fact]
    public void AddItem_MultipleItems_AllPersisted()
    {
        var service = CreateService("inv_multi");
        var characterId = Guid.NewGuid();

        service.AddItem(characterId, new Item { Name = "Minnow", Type = ItemType.Fish, Rarity = 1 });
        service.AddItem(characterId, new Item { Name = "Perch", Type = ItemType.Fish, Rarity = 1 });

        var inventory = service.GetInventory(characterId).ToList();

        Assert.Equal(2, inventory.Count);
    }
}
