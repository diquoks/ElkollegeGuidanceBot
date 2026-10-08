namespace ElkollegeGuidanceBot.Models.Guidance;

public interface IGuidanceBlock
{
    string Title { get; }

    string Hint { get; }

    GuidanceBlockType Type { get; }

    IReadOnlyList<GuidanceBlockQuestion> Questions { get; }
}

public abstract record GuidanceBlock<T> : IGuidanceBlock where T : GuidanceBlockQuestion
{
    public required string Title { get; init; }

    public required string Hint { get; init; }

    public required GuidanceBlockType Type { get; init; }

    public required T[] Questions { get; init; }

    IReadOnlyList<GuidanceBlockQuestion> IGuidanceBlock.Questions => Questions;
}

public enum GuidanceBlockType
{
    Options,
    Binary
}
