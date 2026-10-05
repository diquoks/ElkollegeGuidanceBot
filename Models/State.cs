namespace ElkollegeGuidanceBot.Models;

public record State
{
    public required long UserId { get; init; }
    public StateType Type = StateType.Empty;
    public Dictionary<string, object> Data { get; init; } = new();

    public void Clear()
    {
        Type = StateType.Empty;
        Data.Clear();
    }
}

public enum StateType
{
    Empty,
    PersonalDataAgreement,
    Instructions,
    InputFullName,
    InputPhoneNumber,
    GuidanceTest,
    SelectUserType,
    InputInstitution,
    InputCurrentCourse
}
