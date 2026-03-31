namespace AdventureRpg.Models;

public class Character
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public CharacterClass Class { get; set; }
    public int Level { get; set; } = 1;
    public int Strength { get; set; }
    public int Intelligence { get; set; }
    public int Agility { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
