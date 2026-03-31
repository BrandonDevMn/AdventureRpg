namespace AdventureRpg.DTOs;

public class FishingResultResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Roll { get; set; }
    public int RequiredRoll { get; set; }
    public FishCaughtDto? CaughtItem { get; set; }
}

public class FishCaughtDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Rarity { get; set; }
    public string RarityLabel { get; set; } = string.Empty;
}
