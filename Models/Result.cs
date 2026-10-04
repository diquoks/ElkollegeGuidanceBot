using ClosedXML.Attributes;
using JetBrains.Annotations;

namespace ElkollegeGuidanceBot.Models;

public record Result
{
    [UsedImplicitly]
    [XLColumn(Header = "ID")]
    public int? Id { get; init; }

    [XLColumn(Header = "UserID")]
    public required long UserId { get; init; }

    [XLColumn(Header = "ФИО")]
    public required string FullName { get; init; }

    [XLColumn(Header = "Телефон")]
    public required string PhoneNumber { get; init; }

    [XLColumn(Header = "Тип")]
    public required string UserType { get; init; }

    [XLColumn(Header = "Учебное заведение")]
    public required string Institution { get; init; }

    [XLColumn(Header = "Текущее направление")]
    public required string CurrentCourse { get; init; }

    [XLColumn(Header = "Возможный тип")]
    public required string PossibleType { get; init; }

    [XLColumn(Header = "Время прохождения (UTC)")]
    public required DateTimeOffset Timestamp { get; init; }
}
