namespace ElkollegeGuidanceBot.Models.Guidance;

public abstract record GuidanceBlock
{
    public required string Title { get; init; }

    public required string Hint { get; init; }

    public required GuidanceBlockType Type { get; init; }
}

public abstract record GuidanceBlock<T> : GuidanceBlock where T : GuidanceBlockQuestion
{
    public required T[] Questions { get; init; }
}

public enum GuidanceBlockType
{
    Options,
    Binary
}
