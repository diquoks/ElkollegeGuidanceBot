using JetBrains.Annotations;

namespace ElkollegeGuidanceBot.Models.Guidance.Options;

[UsedImplicitly]
public record GuidanceOptionsQuestion : GuidanceBlockQuestion
{
    public required GuidanceOptionsAnswer[] Answers { get; init; }
}
