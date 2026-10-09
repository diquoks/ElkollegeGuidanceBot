using System.ComponentModel;
using ClosedXML.Attributes;
using JetBrains.Annotations;

namespace ElkollegeGuidanceBot.Models.Database;

public record Result
{
    [UsedImplicitly]
    [XLColumn(Header = "ID")]
    public int? Id { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = "UserID")]
    public required long UserId { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = "ФИО")]
    public required string FullName { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = "Телефон")]
    public required string PhoneNumber { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = "Тип")]
    public required string UserType { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = "Учебное заведение")]
    public required string Institution { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = "Текущее направление")]
    public required string CurrentCourse { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = "Возможный тип")]
    public required string PossibleType { get; init; }

    [XLColumn(Ignore = true)]
    public required DateTimeOffset CreatedTimestamp { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = "Время прохождения")]
    public string Created
    {
        get => CreatedTimestamp.ToString();
        init => CreatedTimestamp = DateTimeOffset.Parse(value);
    }
}

public enum UserType
{
    [Description("Школьник")]
    Schoolkid,

    [Description("Студент СПО")]
    CollegeStudent,

    [Description("Студент вуза")]
    UniversityStudent
}
