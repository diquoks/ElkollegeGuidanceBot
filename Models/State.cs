namespace ElkollegeGuidanceBot.Models;

public record State
{
    public required long UserId;
    public string LastCallback { get; init; } = string.Empty;
    public StateType Type { get; init; } = StateType.Empty;
    public Dictionary<string, object> Data { get; init; } = new();
}

public enum StateType
{
    Empty
}
