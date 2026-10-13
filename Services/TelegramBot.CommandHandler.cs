using ClosedXML.Excel;
using ElkollegeGuidanceBot.Extensions;
using ElkollegeGuidanceBot.Helpers;
using ElkollegeGuidanceBot.Models.Database;
using ElkollegeGuidanceBot.Strings;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;

namespace ElkollegeGuidanceBot.Services;

public partial class TelegramBot
{
    protected override async Task OnCommandAsync(
        Message message,
        string commandName,
        string args,
        CancellationToken cancellationToken = default
    )
    {
        if (
            message.From is null ||
            string.IsNullOrWhiteSpace(message.Text)
        )
            return;

        var state = await _databaseManager.GetUserStateAsync(message.From.Id, cancellationToken);
        var isAdmin = _admins.Contains(message.From.Id);

        _logger.LogBotInteraction(
            message.From!,
            message.Text,
            new
            {
                State = state.Type,
                IsAdmin = isAdmin
            }
        );

        switch (commandName)
        {
            case BotCommands.StartCommand:
            {
                if (state.Type > StateType.Instructions)
                {
                    await Client.SendMessageAsync(
                        chatId: message.Chat.Id,
                        text: MenuStrings.StartHasActiveTest,
                        replyMarkup: BotKeyboards.StartHasActiveTest,
                        cancellationToken: cancellationToken
                    );

                    break;
                }

                await Client.SendMessageAsync(
                    chatId: message.Chat.Id,
                    text: MenuStrings.Start(_botName),
                    parseMode: DefaultParseMode,
                    replyMarkup: BotKeyboards.Start,
                    cancellationToken: cancellationToken
                );

                break;
            }
            case BotCommands.ExportCommand:
            {
                if (!isAdmin)
                    break;

                var allResults = await _databaseManager.GetAllResultsAsync(cancellationToken);

                using var workbook = new XLWorkbook();

                workbook.Worksheets
                    .Add(OtherStrings.Result.WorksheetName)
                    .FirstCell()
                    .InsertTable(allResults);

                workbook.Worksheets
                    .Add(OtherStrings.GuidanceType.WorksheetName)
                    .FirstCell()
                    .InsertTable(_guidanceProvider.Test.Types);

                foreach (var worksheet in workbook.Worksheets)
                    worksheet
                        .Columns()
                        .AdjustToContents();

                await using var workbookStream = new MemoryStream();
                workbook.SaveAs(workbookStream);
                workbookStream.Position = 0;

                await Client.SendDocumentAsync(
                    chatId: message.Chat.Id,
                    document: new InputFile(workbookStream, "ElkollegeGuidanceExport.xlsx"),
                    cancellationToken: cancellationToken
                );

                break;
            }
        }
    }
}
