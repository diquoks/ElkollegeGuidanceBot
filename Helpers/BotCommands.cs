using Telegram.BotAPI.AvailableTypes;

namespace ElkollegeGuidanceBot.Helpers;

public static class BotCommands
{
    public const string StartCommand = "start";

    private static BotCommand Start =>
        new(StartCommand, "Пройти тестирование");

    public const string ExportCommand = "export";

    private static BotCommand Export =>
        new(ExportCommand, "Экспортировать БД");

    public static BotCommand[] AllCommands =>
    [
        Start,
        Export
    ];
}
