using AdventureRpg.Controllers;
using AdventureRpg.Models;
using AdventureRpg.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace UnitTests.Controllers;

public class InventoryControllerTests
{
    private readonly Mock<IInventoryService> _inventoryMock = new();
    private readonly Mock<ICharacterService> _characterMock = new();

    private InventoryController CreateController() =>
        new(_inventoryMock.Object, _characterMock.Object);

    [Fact]
    public void GetInventory_CharacterNotFound_ReturnsNotFound()
    {
        _characterMock.Setup(s => s.GetById(It.IsAny<Guid>())).Returns((Character?)null);

        var result = CreateController().GetInventory(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetInventory_CharacterFound_ReturnsOkWithItems()
    {
        var character = new Character { Name = "Hero", Class = CharacterClass.Warrior };
        var items = new List<Item>
        {
            new() { Name = "Minnow", Type = ItemType.Fish, Rarity = 1 }
        };

        _characterMock.Setup(s => s.GetById(character.Id)).Returns(character);
        _inventoryMock.Setup(s => s.GetInventory(character.Id)).Returns(items);

        var result = CreateController().GetInventory(character.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
