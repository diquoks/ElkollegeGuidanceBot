using System.ComponentModel;
using ClosedXML.Attributes;
using ElkollegeGuidanceBot.Strings;
using JetBrains.Annotations;

namespace ElkollegeGuidanceBot.Models.Database;

public record Result
{
    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.Id)]
    public int? Id { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.UserId)]
    public required long UserId { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.FullName)]
    public required string FullName { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.PhoneNumber)]
    public required string PhoneNumber { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.Type)]
    public required string UserType { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.Institution)]
    public required string Institution { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.CurrentCourse)]
    public required string CurrentCourse { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.PossibleType)]
    public required string PossibleType { get; init; }

    [XLColumn(Ignore = true)]
    public required DateTimeOffset CreatedTimestamp { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.Created)]
    public string Created
    {
        get => CreatedTimestamp.ToString();
        init => CreatedTimestamp = DateTimeOffset.Parse(value);
    }
}

public enum UserType
{
    [Description(OtherStrings.UserType.Schoolkid)]
    Schoolkid,

    [Description(OtherStrings.UserType.CollegeStudent)]
    CollegeStudent,

    [Description(OtherStrings.UserType.UniversityStudent)]
    UniversityStudent
}
