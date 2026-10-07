namespace ElkollegeGuidanceBot.Models.Guidance;

public abstract record GuidanceBlockQuestion
{
    public required string Question { get; init; }
}
