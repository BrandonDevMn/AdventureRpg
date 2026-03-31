using AdventureRpg.DTOs;
using AdventureRpg.Models;
using AdventureRpg.Services;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class CharacterServiceTests
{
    private CharacterService CreateService(string dbName) =>
        new(DbContextFactory.Create(dbName));

    [Theory]
    [InlineData(CharacterClass.Warrior, 10, 4, 6)]
    [InlineData(CharacterClass.Mage, 3, 12, 5)]
    [InlineData(CharacterClass.Rogue, 6, 6, 11)]
    [InlineData(CharacterClass.Ranger, 7, 5, 9)]
    public void Create_AssignsCorrectBaseStatsByClass(CharacterClass cls, int str, int intel, int agi)
    {
        var service = CreateService($"char_stats_{cls}");
        var request = new CreateCharacterRequest { Name = "Hero", Class = cls };

        var character = service.Create(request);

        Assert.Equal(str, character.Strength);
        Assert.Equal(intel, character.Intelligence);
        Assert.Equal(agi, character.Agility);
    }

    [Fact]
    public void Create_PersistsCharacterWithCorrectNameAndClass()
    {
        var service = CreateService("char_persist");
        var request = new CreateCharacterRequest { Name = "Aldric", Class = CharacterClass.Warrior };

        var character = service.Create(request);

        Assert.Equal("Aldric", character.Name);
        Assert.Equal(CharacterClass.Warrior, character.Class);
        Assert.Equal(1, character.Level);
        Assert.NotEqual(Guid.Empty, character.Id);
    }

    [Fact]
    public void GetById_ExistingCharacter_ReturnsCharacter()
    {
        var service = CreateService("char_getbyid_found");
        var created = service.Create(new CreateCharacterRequest { Name = "Elara", Class = CharacterClass.Mage });

        var result = service.GetById(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public void GetById_NonExistentCharacter_ReturnsNull()
    {
        var service = CreateService("char_getbyid_notfound");

        var result = service.GetById(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void GetAll_ReturnsAllCreatedCharacters()
    {
        var service = CreateService("char_getall");
        service.Create(new CreateCharacterRequest { Name = "A", Class = CharacterClass.Warrior });
        service.Create(new CreateCharacterRequest { Name = "B", Class = CharacterClass.Rogue });

        var all = service.GetAll().ToList();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void GetAll_EmptyDb_ReturnsEmptyCollection()
    {
        var service = CreateService("char_getall_empty");

        var all = service.GetAll().ToList();

        Assert.Empty(all);
    }
}
