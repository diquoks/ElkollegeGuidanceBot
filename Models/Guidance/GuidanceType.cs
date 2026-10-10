using ClosedXML.Attributes;
using ElkollegeGuidanceBot.Strings;
using JetBrains.Annotations;

namespace ElkollegeGuidanceBot.Models.Guidance;

[UsedImplicitly]
public record GuidanceType
{
    [XLColumn(Header = OtherStrings.GuidanceType.Class)]
    public required string Class { get; init; }

    [XLColumn(Header = OtherStrings.GuidanceType.Name)]
    public required string Name { get; init; }

    [XLColumn(Ignore = true)]
    public required string[] Professions { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.GuidanceType.Professions)]
    public string ProfessionsString => string.Join(", ", Professions);
}
