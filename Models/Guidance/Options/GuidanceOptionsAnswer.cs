using JetBrains.Annotations;

namespace ElkollegeGuidanceBot.Models.Guidance.Options;

[UsedImplicitly]
public record GuidanceOptionsAnswer
{
    public required string Answer { get; init; }

    public required int TypeIndex { get; init; }
}
