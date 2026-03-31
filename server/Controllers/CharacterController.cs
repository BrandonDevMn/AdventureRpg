using System.Security.Claims;
using AdventureRpg.DTOs;
using AdventureRpg.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdventureRpg.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/character")]
public class CharacterController(ICharacterService characterService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    public IActionResult Create([FromBody] CreateCharacterRequest request)
    {
        var character = characterService.Create(request, UserId);
        return CreatedAtAction(nameof(GetById), new { id = character.Id }, character);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var character = characterService.GetById(id, UserId);
        if (character is null)
            return NotFound();
        return Ok(character);
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(characterService.GetAll(UserId));
}
