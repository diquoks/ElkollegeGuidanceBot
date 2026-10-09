using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace ElkollegeGuidanceBot.Models.Database;

public record State
{
    public required long UserId { get; init; }

    public StateType Type { get; set; } = StateType.Empty;

    public StateData Data { get; private set; } = new();

    [UsedImplicitly]
    public string DataJson
    {
        get => JsonSerializer.Serialize(Data);
        set => Data = JsonSerializer.Deserialize<StateData>(value) ?? new StateData();
    }

    public void Clear()
    {
        Type = StateType.Empty;
        Data = new StateData();
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

public record StateData
{
    public string NextExpectedCallback { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FullName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PhoneNumber { get; set; }

    public int PhoneNumberRetries { get; set; }

    public Dictionary<int, int> GuidanceTypeRatings { get; init; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserType? UserType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Institution { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CurrentCourse { get; set; }
}
