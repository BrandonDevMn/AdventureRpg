using AdventureRpg.Controllers;
using AdventureRpg.DTOs;
using AdventureRpg.Models;
using AdventureRpg.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace UnitTests.Controllers;

public class CharacterControllerTests
{
    private readonly Mock<ICharacterService> _serviceMock = new();
    private CharacterController CreateController() => new(_serviceMock.Object);

    [Fact]
    public void Create_ReturnsCreatedAtAction_WithCharacter()
    {
        var character = new Character { Name = "Aldric", Class = CharacterClass.Warrior };
        _serviceMock.Setup(s => s.Create(It.IsAny<CreateCharacterRequest>())).Returns(character);

        var result = CreateController().Create(new CreateCharacterRequest { Name = "Aldric", Class = CharacterClass.Warrior });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(character, created.Value);
    }

    [Fact]
    public void GetById_CharacterExists_ReturnsOk()
    {
        var character = new Character { Name = "Elara", Class = CharacterClass.Mage };
        _serviceMock.Setup(s => s.GetById(character.Id)).Returns(character);

        var result = CreateController().GetById(character.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(character, ok.Value);
    }

    [Fact]
    public void GetById_CharacterNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetById(It.IsAny<Guid>())).Returns((Character?)null);

        var result = CreateController().GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetAll_ReturnsOkWithCharacters()
    {
        var characters = new List<Character>
        {
            new() { Name = "A", Class = CharacterClass.Warrior },
            new() { Name = "B", Class = CharacterClass.Rogue }
        };
        _serviceMock.Setup(s => s.GetAll()).Returns(characters);

        var result = CreateController().GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(characters, ok.Value);
    }
}
