using AdventureRpg.Controllers;
using AdventureRpg.DTOs;
using AdventureRpg.Models;
using AdventureRpg.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace UnitTests.Controllers;

public class FishingControllerTests
{
    private readonly Mock<IFishingService> _fishingMock = new();
    private readonly Mock<ICharacterService> _characterMock = new();

    private FishingController CreateController() =>
        new(_fishingMock.Object, _characterMock.Object);

    [Fact]
    public void Cast_CharacterNotFound_ReturnsNotFound()
    {
        _characterMock.Setup(s => s.GetById(It.IsAny<Guid>())).Returns((Character?)null);

        var result = CreateController().Cast(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void Cast_CharacterFound_ReturnsFishingResult()
    {
        var character = new Character { Name = "Hero", Class = CharacterClass.Ranger, Agility = 9 };
        var fishingResult = new FishingResultResponse { Success = true, Message = "You reeled in a Bass!" };

        _characterMock.Setup(s => s.GetById(character.Id)).Returns(character);
        _fishingMock.Setup(s => s.Cast(character)).Returns(fishingResult);

        var result = CreateController().Cast(character.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(fishingResult, ok.Value);
    }
}
