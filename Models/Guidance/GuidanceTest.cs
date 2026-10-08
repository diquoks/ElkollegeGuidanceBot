using JetBrains.Annotations;

namespace ElkollegeGuidanceBot.Models.Guidance;

[UsedImplicitly]
public record GuidanceTest
{
    public required string Instruction { get; init; }

    public required GuidanceType[] Types { get; init; }

    public required IGuidanceBlock[] Blocks { get; init; }
}
