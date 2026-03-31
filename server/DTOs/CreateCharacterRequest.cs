using AdventureRpg.Models;
using System.ComponentModel.DataAnnotations;

namespace AdventureRpg.DTOs;

public class CreateCharacterRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(24)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public CharacterClass Class { get; set; }
}
