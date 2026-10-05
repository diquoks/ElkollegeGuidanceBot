using ElkollegeGuidanceBot.Extensions;
using ElkollegeGuidanceBot.Models;
using ElkollegeGuidanceBot.Strings;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
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
        if (
            message.From is null ||
            message.From.Id == TelegramConstants.TelegramId ||
            string.IsNullOrWhiteSpace(message.Text)
        )
            return;

        if (BotCommandParser.TryParse(message, out _))
        {
            await base.OnMessageAsync(message, cancellationToken);
            return;
        }

        var state = await _databaseManager.GetUserStateAsync(message.From.Id, cancellationToken);

        _logger.LogBotInteraction(message.From, $"\"{message.Text}\"", new { State = state.Type });

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (state.Type)
        {
            case StateType.InputFullName:
                state.Type = StateType.InputPhoneNumber;
                state.Data[StateDataStrings.FullName] = message.Text;

                await Client.SendMessageAsync(
                    chatId: message.Chat.Id,
                    text: MenuStrings.InputPhoneNumber,
                    cancellationToken: cancellationToken
                );

                break;
            case StateType.InputPhoneNumber:
                throw new NotImplementedException();
        }

        await _databaseManager.UpsertStateAsync(state, cancellationToken);

        await base.OnMessageAsync(message, cancellationToken);
    }
}
