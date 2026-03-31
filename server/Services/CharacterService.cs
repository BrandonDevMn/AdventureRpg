using AdventureRpg.Data;
using AdventureRpg.Models;
using AdventureRpg.DTOs;

namespace AdventureRpg.Services;

public interface ICharacterService
{
    Character Create(CreateCharacterRequest request, string userId);
    Character? GetById(Guid id, string userId);
    IEnumerable<Character> GetAll(string userId);
}

public class CharacterService(AppDbContext db) : ICharacterService
{
    private static readonly Dictionary<CharacterClass, (int str, int intel, int agi)> BaseStats = new()
    {
        { CharacterClass.Warrior,  (str: 10, intel: 4,  agi: 6)  },
        { CharacterClass.Mage,     (str: 3,  intel: 12, agi: 5)  },
        { CharacterClass.Rogue,    (str: 6,  intel: 6,  agi: 11) },
        { CharacterClass.Ranger,   (str: 7,  intel: 5,  agi: 9)  },
    };

    public Character Create(CreateCharacterRequest request, string userId)
    {
        var stats = BaseStats[request.Class];
        var character = new Character
        {
            Name = request.Name,
            Class = request.Class,
            Strength = stats.str,
            Intelligence = stats.intel,
            Agility = stats.agi,
            UserId = userId
        };
        db.Characters.Add(character);
        db.SaveChanges();
        return character;
    }

    public Character? GetById(Guid id, string userId) =>
        db.Characters.FirstOrDefault(c => c.Id == id && c.UserId == userId);

    public IEnumerable<Character> GetAll(string userId) =>
        db.Characters.Where(c => c.UserId == userId).ToList();
}
