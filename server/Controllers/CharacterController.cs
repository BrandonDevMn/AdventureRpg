using Microsoft.AspNetCore.Mvc;
using AdventureRpg.DTOs;
using AdventureRpg.Services;

namespace AdventureRpg.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CharacterController : ControllerBase
{
    private readonly ICharacterService _characterService;

    public CharacterController(ICharacterService characterService)
    {
        _characterService = characterService;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateCharacterRequest request)
    {
        var character = _characterService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = character.Id }, character);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var character = _characterService.GetById(id);
        if (character is null)
            return NotFound();
        return Ok(character);
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_characterService.GetAll());
}
