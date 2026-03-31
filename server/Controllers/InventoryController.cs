using System.Security.Claims;
using AdventureRpg.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureRpg.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/inventory")]
public class InventoryController(IInventoryService inventoryService, ICharacterService characterService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("{characterId:guid}")]
    public IActionResult GetInventory(Guid characterId)
    {
        var character = characterService.GetById(characterId, UserId);
        if (character is null)
            return NotFound(new { message = "Character not found." });

        var items = inventoryService.GetInventory(characterId);
        return Ok(new { characterId, items });
    }
}
