namespace AdventureRpg.Models;

public class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CharacterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Rarity { get; set; } // 1=Common, 2=Uncommon, 3=Rare, 4=Legendary
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
}
