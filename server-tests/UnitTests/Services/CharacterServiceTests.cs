using AdventureRpg.DTOs;
using AdventureRpg.Models;
using AdventureRpg.Services;
using UnitTests.Helpers;
using Xunit;

namespace UnitTests.Services;

public class CharacterServiceTests
{
    private const string UserId = "user-abc";

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

        var character = service.Create(request, UserId);

        Assert.Equal(str, character.Strength);
        Assert.Equal(intel, character.Intelligence);
        Assert.Equal(agi, character.Agility);
    }

    [Fact]
    public void Create_PersistsCharacterWithCorrectNameClassAndUserId()
    {
        var service = CreateService("char_persist");
        var request = new CreateCharacterRequest { Name = "Aldric", Class = CharacterClass.Warrior };

        var character = service.Create(request, UserId);

        Assert.Equal("Aldric", character.Name);
        Assert.Equal(CharacterClass.Warrior, character.Class);
        Assert.Equal(UserId, character.UserId);
        Assert.Equal(1, character.Level);
        Assert.NotEqual(Guid.Empty, character.Id);
    }

    [Fact]
    public void GetById_ExistingCharacterForUser_ReturnsCharacter()
    {
        var service = CreateService("char_getbyid_found");
        var created = service.Create(new CreateCharacterRequest { Name = "Elara", Class = CharacterClass.Mage }, UserId);

        var result = service.GetById(created.Id, UserId);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public void GetById_CharacterBelongsToOtherUser_ReturnsNull()
    {
        var service = CreateService("char_getbyid_otheruser");
        var created = service.Create(new CreateCharacterRequest { Name = "Elara", Class = CharacterClass.Mage }, "other-user");

        var result = service.GetById(created.Id, UserId);

        Assert.Null(result);
    }

    [Fact]
    public void GetById_NonExistentCharacter_ReturnsNull()
    {
        var service = CreateService("char_getbyid_notfound");

        var result = service.GetById(Guid.NewGuid(), UserId);

        Assert.Null(result);
    }

    [Fact]
    public void GetAll_ReturnsOnlyCurrentUsersCharacters()
    {
        var service = CreateService("char_getall_scoped");
        service.Create(new CreateCharacterRequest { Name = "Mine", Class = CharacterClass.Warrior }, UserId);
        service.Create(new CreateCharacterRequest { Name = "Theirs", Class = CharacterClass.Rogue }, "other-user");

        var all = service.GetAll(UserId).ToList();

        Assert.Single(all);
        Assert.Equal("Mine", all[0].Name);
    }

    [Fact]
    public void GetAll_EmptyDb_ReturnsEmptyCollection()
    {
        var service = CreateService("char_getall_empty");

        var all = service.GetAll(UserId).ToList();

        Assert.Empty(all);
    }

    [Fact]
    public void Delete_OwnedCharacter_RemovesCharacterAndReturnsTrue()
    {
        var service = CreateService("char_delete");
        var created = service.Create(new CreateCharacterRequest { Name = "Doomed", Class = CharacterClass.Warrior }, UserId);

        var result = service.Delete(created.Id, UserId);

        Assert.True(result);
        Assert.Null(service.GetById(created.Id, UserId));
    }

    [Fact]
    public void Delete_CharacterBelongsToOtherUser_ReturnsFalse()
    {
        var service = CreateService("char_delete_otheruser");
        var created = service.Create(new CreateCharacterRequest { Name = "Theirs", Class = CharacterClass.Rogue }, "other-user");

        var result = service.Delete(created.Id, UserId);

        Assert.False(result);
    }

    [Fact]
    public void Delete_NonExistentCharacter_ReturnsFalse()
    {
        var service = CreateService("char_delete_notfound");

        var result = service.Delete(Guid.NewGuid(), UserId);

        Assert.False(result);
    }
}
