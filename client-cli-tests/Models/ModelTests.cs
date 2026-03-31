using AdventureRpgCli.Models;
using Xunit;

namespace client_cli_tests.Models;

public class CharacterClassNameTests
{
    [Theory]
    [InlineData(0, "Warrior")]
    [InlineData(1, "Mage")]
    [InlineData(2, "Rogue")]
    [InlineData(3, "Ranger")]
    [InlineData(99, "Unknown")]
    public void ClassName_ReturnsCorrectLabel(int classId, string expected)
    {
        var character = new Character(Guid.NewGuid(), "Hero", classId, 1, 10, 5, 8);
        Assert.Equal(expected, character.ClassName);
    }
}

public class ItemRarityLabelTests
{
    private static Item MakeItem(int rarity) =>
        new(Guid.NewGuid(), "Thing", "Fish", "desc", rarity, DateTime.UtcNow);

    [Theory]
    [InlineData(1, "Common")]
    [InlineData(2, "Uncommon")]
    [InlineData(3, "Rare")]
    [InlineData(4, "Legendary")]
    [InlineData(99, "Unknown")]
    public void RarityLabel_ReturnsCorrectLabel(int rarity, string expected)
    {
        Assert.Equal(expected, MakeItem(rarity).RarityLabel);
    }
}
