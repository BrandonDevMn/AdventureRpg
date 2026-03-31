using Microsoft.AspNetCore.Mvc;
using AdventureRpg.Services;

namespace AdventureRpg.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ICharacterService _characterService;

    public InventoryController(IInventoryService inventoryService, ICharacterService characterService)
    {
        _inventoryService = inventoryService;
        _characterService = characterService;
    }

    [HttpGet("{characterId:guid}")]
    public IActionResult GetInventory(Guid characterId)
    {
        var character = _characterService.GetById(characterId);
        if (character is null)
            return NotFound(new { message = "Character not found." });

        var items = _inventoryService.GetInventory(characterId);
        return Ok(new { characterId, items });
    }
}
