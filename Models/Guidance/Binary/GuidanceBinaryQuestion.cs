using JetBrains.Annotations;

namespace ElkollegeGuidanceBot.Models.Guidance.Binary;

[UsedImplicitly]
public record GuidanceBinaryQuestion : GuidanceBlockQuestion
{
    public required int TypeIndex { get; init; }
}
