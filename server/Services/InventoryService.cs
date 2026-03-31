using AdventureRpg.Data;
using AdventureRpg.Models;
using Microsoft.EntityFrameworkCore;

namespace AdventureRpg.Services;

public interface IInventoryService
{
    IEnumerable<Item> GetInventory(Guid characterId);
    void AddItem(Guid characterId, Item item);
}

public class InventoryService(AppDbContext db) : IInventoryService
{
    public IEnumerable<Item> GetInventory(Guid characterId) =>
        db.Items.Where(i => i.CharacterId == characterId).ToList();

    public void AddItem(Guid characterId, Item item)
    {
        item.CharacterId = characterId;
        db.Items.Add(item);
        db.SaveChanges();
    }
}
