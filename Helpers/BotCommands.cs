using ElkollegeGuidanceBot.Strings;
using Telegram.BotAPI.AvailableTypes;

namespace ElkollegeGuidanceBot.Helpers;

public static class BotCommands
{
    public const string StartCommand = "start";

    public const string ExportCommand = "export";

    public static BotCommand[] AllCommands =>
    [
        new(StartCommand, OtherStrings.Commands.Start),
        new(ExportCommand, OtherStrings.Commands.Export)
    ];
}
