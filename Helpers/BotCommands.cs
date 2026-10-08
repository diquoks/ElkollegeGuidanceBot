using Telegram.BotAPI.AvailableTypes;

namespace ElkollegeGuidanceBot.Helpers;

public static class BotCommands
{
    public const string StartCommand = "start";

    public const string ExportCommand = "export";

    public static BotCommand[] AllCommands =>
    [
        new(StartCommand, "Пройти тестирование"),
        new(ExportCommand, "Экспортировать БД")
    ];
}
