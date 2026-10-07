using ClosedXML.Attributes;
using JetBrains.Annotations;

namespace ElkollegeGuidanceBot.Models.Guidance;

[UsedImplicitly]
public record GuidanceType
{
    [XLColumn(Header = "Тип")]
    public required string Class { get; init; }

    [XLColumn(Header = "Имя")]
    public required string Name { get; init; }

    [XLColumn(Ignore = true)]
    public required string[] Professions { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = "Профессии")]
    public string ProfessionsString => string.Join(", ", Professions);
}
