using Microsoft.AspNetCore.Mvc;
using AdventureRpg.Services;

namespace AdventureRpg.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FishingController : ControllerBase
{
    private readonly IFishingService _fishingService;
    private readonly ICharacterService _characterService;

    public FishingController(IFishingService fishingService, ICharacterService characterService)
    {
        _fishingService = fishingService;
        _characterService = characterService;
    }

    /// <summary>
    /// Cast the fishing line for a character. Returns the dice roll result and any fish caught.
    /// </summary>
    [HttpPost("{characterId:guid}/cast")]
    public IActionResult Cast(Guid characterId)
    {
        var character = _characterService.GetById(characterId);
        if (character is null)
            return NotFound(new { message = "Character not found." });

        var result = _fishingService.Cast(character);
        return Ok(result);
    }
}
