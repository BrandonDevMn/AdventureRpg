using AdventureRpg.Controllers;
using AdventureRpg.DTOs;
using AdventureRpg.Models;
using AdventureRpg.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Controllers;

public class CharacterControllerTests
{
    private const string UserId = "user-abc";
    private readonly Mock<ICharacterService> _serviceMock = new();

    private CharacterController CreateController()
    {
        var controller = new CharacterController(_serviceMock.Object);
        controller.SetUser(UserId);
        return controller;
    }

    [Fact]
    public void Create_ReturnsCreatedAtAction_WithCharacter()
    {
        var character = new Character { Name = "Aldric", Class = CharacterClass.Warrior, UserId = UserId };
        _serviceMock.Setup(s => s.Create(It.IsAny<CreateCharacterRequest>(), UserId)).Returns(character);

        var result = CreateController().Create(new CreateCharacterRequest { Name = "Aldric", Class = CharacterClass.Warrior });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(character, created.Value);
    }

    [Fact]
    public void GetById_CharacterExists_ReturnsOk()
    {
        var character = new Character { Name = "Elara", Class = CharacterClass.Mage, UserId = UserId };
        _serviceMock.Setup(s => s.GetById(character.Id, UserId)).Returns(character);

        var result = CreateController().GetById(character.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(character, ok.Value);
    }

    [Fact]
    public void GetById_CharacterNotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetById(It.IsAny<Guid>(), UserId)).Returns((Character?)null);

        var result = CreateController().GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetAll_ReturnsOkWithCharacters()
    {
        var characters = new List<Character>
        {
            new() { Name = "A", Class = CharacterClass.Warrior, UserId = UserId },
            new() { Name = "B", Class = CharacterClass.Rogue, UserId = UserId }
        };
        _serviceMock.Setup(s => s.GetAll(UserId)).Returns(characters);

        var result = CreateController().GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(characters, ok.Value);
    }
}
