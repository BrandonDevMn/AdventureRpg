using System.Security.Claims;
using AdventureRpg.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureRpg.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/fishing")]
public class FishingController(IFishingService fishingService, ICharacterService characterService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("{characterId:guid}/cast")]
    public IActionResult Cast(Guid characterId)
    {
        var character = characterService.GetById(characterId, UserId);
        if (character is null)
            return NotFound(new { message = "Character not found." });

        var result = fishingService.Cast(character);
        return Ok(result);
    }
}
