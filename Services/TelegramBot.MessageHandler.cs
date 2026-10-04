using ElkollegeGuidanceBot.Extensions;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.Extensions.Commands;

namespace ElkollegeGuidanceBot.Services;

public partial class TelegramBot
{
    protected override async Task OnMessageAsync(
        Message message,
        CancellationToken cancellationToken = default
    )
    {
        if (message.From?.Id == TelegramConstants.TelegramId)
            return;

        if (string.IsNullOrEmpty(message.Text ?? message.Caption))
            return;

        if (BotCommandParser.TryParse(message, out _))
        {
            await base.OnMessageAsync(message, cancellationToken);
            return;
        }

        var state = await _databaseManager.GetUserStateAsync(message.From!.Id, cancellationToken);

        _logger.LogBotInteraction(message.From!, $"\"{message.Text}\"", new { State = state.Type });

        await base.OnMessageAsync(message, cancellationToken);
    }
}
