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
    public required string? FullName { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.PhoneNumber)]
    public required string? PhoneNumber { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.Status)]
    public required string? UserStatus { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.Institution)]
    public required string? Institution { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.CurrentCourse)]
    public required string? CurrentCourse { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.PossibleType)]
    public required string PossibleType { get; init; }

    [UsedImplicitly]
    [XLColumn(Header = OtherStrings.Result.TypeRatings)]
    public required string TypeRatings { get; init; }

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

public enum UserStatus
{
    [Description(OtherStrings.UserStatus.Schoolkid)]
    Schoolkid = 1,

    [Description(OtherStrings.UserStatus.CollegeStudent)]
    CollegeStudent = 2,

    [Description(OtherStrings.UserStatus.UniversityStudent)]
    UniversityStudent = 3,

    [Description(OtherStrings.UserStatus.Other)]
    Other = 0
}
