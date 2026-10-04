using ClosedXML.Excel;
using ElkollegeGuidanceBot.Extensions;
using ElkollegeGuidanceBot.Helpers;
using ElkollegeGuidanceBot.Models;
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
        var isAdmin = _admins.Contains(message.From!.Id);
        var state = await _databaseManager.GetUserStateAsync(message.From!.Id, cancellationToken);

        _logger.LogBotInteraction(message.From!, $"/{commandName}", new { IsAdmin = isAdmin });

        switch (commandName)
        {
            case BotCommands.StartCommand:
                if (state.Type != StateType.Empty)
                {
                    await Client.SendMessageAsync(
                        message.Chat.Id,
                        MenuStrings.StartHasActiveTest,
                        replyMarkup: BotKeyboards.StartHasActiveTest(state.LastCallback),
                        cancellationToken: cancellationToken
                    );

                    break;
                }

                await Client.SendMessageAsync(
                    message.Chat.Id,
                    MenuStrings.Start(_botName),
                    parseMode: DefaultParseMode,
                    replyMarkup: BotKeyboards.Start,
                    cancellationToken: cancellationToken
                );

                break;

            case BotCommands.ExportCommand:
                if (!isAdmin)
                    break;

                var allResults = await _databaseManager.GetAllResultsAsync(cancellationToken);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add();
                    worksheet.FirstCell().InsertTable(allResults);
                    worksheet.Columns().AdjustToContents();

                    await using var workbookStream = new MemoryStream();
                    workbook.SaveAs(workbookStream);
                    workbookStream.Position = 0;

                    await Client.SendDocumentAsync(
                        message.Chat.Id,
                        new InputFile(workbookStream, "ElkollegeGuidanceExport.xlsx"),
                        cancellationToken: cancellationToken
                    );
                }

                break;
        }
    }
}
